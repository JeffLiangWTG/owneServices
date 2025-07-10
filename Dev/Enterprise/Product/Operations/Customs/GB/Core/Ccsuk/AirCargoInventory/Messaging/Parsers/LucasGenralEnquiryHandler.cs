using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class LucasGenralEnquiryHandler
	{
		public LucasGenralEnquiryHandler(GenralMessage genral, EDIMessage inboundMessage, string payload)
		{
			this.genral = genral;
			this.inboundMessage = inboundMessage;
			this.lucasQueryString = payload;
			children = Array.Empty<ICcsukCusAwb>();
		}

		internal void DoAllProcessing()
		{
			try
			{
				FindConsignmentSerialNumberFromInboundGenral();
				SetCommonAccessReferenceFromUnh();
				FindConsignmentFromLucasRequest();
				FindConsignmentsChildren();
				PrepareResponseMessages();
				UpdateInboundMessage();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LucalGenralResponseMaker.New(StatusOfTheRequest.InternalError, inboundMessage, lucasQueryString, null, null, objectRequestType, commonAccessReference).MakeAndSendAllResponses();
				inboundMessage.EM_Status = EDIMessage.Status.Error;
			}
		}

		void SetCommonAccessReferenceFromUnh()
		{
			commonAccessReference = genral.UNH[0].CommonAccessReference;
		}

		void FindConsignmentSerialNumberFromInboundGenral()
		{
			if (requestStatus != StatusOfTheRequest.OK)
			{
				return;
			}

			if (lucasQueryString == string.Empty)
			{
				requestStatus = StatusOfTheRequest.BadRequest;
			}

			if (lucasQueryString.Length == 0 || !lucasQueryString.Contains("="))
			{
				requestStatus = StatusOfTheRequest.BadRequest;
				return;
			}
			ZString[] words = lucasQueryString.Split('=');
			objectRequestType = (EnquiryObjectTypes)Enum.Parse(typeof(EnquiryObjectTypes), words[0]);
			objectSerialNumber = words[1];
		}

		void FindConsignmentFromLucasRequest()
		{
			if (requestStatus != StatusOfTheRequest.OK)
			{
				return;
			}

			switch (objectRequestType)
			{
				case EnquiryObjectTypes.MAWB:
					if (!objectSerialNumber.Contains("-"))
					{
						requestStatus = StatusOfTheRequest.BadRequest;
						return;
					}
					objectSerialNumber = objectSerialNumber.Replace("-", "");
					consignment = GetMawbConsignmentFromCusMAWBOrConsol(objectSerialNumber);
					break;

				case EnquiryObjectTypes.HAWB:
					switch (objectSerialNumber.Length)
					{
						case 0:
						case 1:
						case 2:
						case 3:
						case 4:
						case 5:
						case 6:
							requestStatus = StatusOfTheRequest.BadRequest;
							return;

						default:
							var hawbs = new CusHAWB.Loader(inboundMessage.Factory).FindAllHawbs(objectSerialNumber, objectSerialNumber.Length == 8);
							var allHawbsAndSplits = new List<IBusiness>();
							allHawbsAndSplits.AddRange(hawbs);
							foreach (CusHAWB hawb in hawbs)
							{
								if (hawb.HasSplits)
								{
									allHawbsAndSplits.AddRange(hawb.Splits.Cast<SplitConsignment>().OrderBy(s => s.SplitReference));
								}
							}
							children = allHawbsAndSplits.ToArray();
							return;
					}

				case EnquiryObjectTypes.DUCR:
					consignment = inboundMessage.Factory.LoadTop1<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.CH_BGMReference, objectSerialNumber));
					break;

				case EnquiryObjectTypes.SHPR:
				case EnquiryObjectTypes.CNSE:
					return;

				default:
					requestStatus = StatusOfTheRequest.BadRequest;
					return;
			}

			if (consignment == null)
			{
				requestStatus = StatusOfTheRequest.NoRecordFound;
			}
		}

		IBusiness GetMawbConsignmentFromCusMAWBOrConsol(ZString objectSerialNumber)
		{
			// 1. Try loading CusMAWB
			IBusiness result = new CusMAWB.Loader(inboundMessage.Factory).FindFromMawbNumber(objectSerialNumber);

			// 2. Try loading ForwardingConsol
			if (result == null)
			{
				var consolQuery = new ZQuery(JobConsolSchema.JK_MasterBillNum, objectSerialNumber);
				consolQuery.OrderBy = JobConsolSchema.JK_SystemCreateTimeUtc.Name + " DESC";
				result = inboundMessage.Factory.LoadTop1<ForwardingConsol>(consolQuery);
			}
			return result;
		}

		void FindConsignmentsChildren()
		{
			if (requestStatus != StatusOfTheRequest.OK)
			{
				return;
			}

			if (consignment == null && children != null && children.Length > 0)
			{
				return;
			}

			if (objectRequestType == EnquiryObjectTypes.MAWB)
			{
				if (consignment is CusMAWB)
				{
					FindChildrenAndGrandChildrenOfMawbOrBasic_Import(consignment, out children);
				}
				else if (consignment is ForwardingConsol)
				{
					FindChildrenAndGrandChildrenOfConsol_Export(consignment, out children);
				}
			}

			if (objectRequestType == EnquiryObjectTypes.HAWB)
			{
				var hawb = consignment as CusHAWB;
				var iAwbChildren = new List<ICcsukCusAwb>();
				if (hawb != null)
				{
					foreach (SplitHouse splitHouse in hawb.Splits)
					{
						iAwbChildren.Add(splitHouse);
					}
				}
				else
				{
					// consignment unset, children already full of hawbs
					foreach (CusHAWB matchingHawb in children)
					{
						foreach (SplitHouse splitHouse in matchingHawb.Splits)
						{
							iAwbChildren.Add(splitHouse);
						}
					}
				}
				iAwbChildren.Sort(new ICcsukCusAwbAscendingOrderComparer());
				children = iAwbChildren.ToArray();
			}

			if (objectRequestType == EnquiryObjectTypes.DUCR)
			{
				var entry = consignment as CusEntryHeader;
				if (entry != null)
				{
					ZQuery query = null;
					if (objectSerialNumber.Contains("/"))
					{
						query = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, objectSerialNumber.Replace(" ", ""));
					}
					else
					{
						query = new ZQuery();
						if (consignment != null)
						{
							query.AddToFilter(CusEntryHeaderSchema.PK, SQLComparisonOperator.NotEqual, ((BusinessObject)consignment).PK);
						}
						var query3 = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, objectSerialNumber);
						var query2 = new ZQuery(CusEntryHeaderSchema.CH_BGMReference, SQLComparisonOperator.StartsWith, objectSerialNumber);
						query2.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, SQLComparisonOperator.Contains, "/");
						query3.AddToFilter(query2, JoinCondition.Or);
						query.AddToFilter(query3);
					}
					query.OrderBy = CusEntryHeaderSchema.CH_BGMReference.Name;
					children = inboundMessage.Factory.Load<CusEntryHeader>(query);
				}
			}

			if (objectRequestType == EnquiryObjectTypes.SHPR || objectRequestType == EnquiryObjectTypes.CNSE)
			{
				children = inboundMessage.Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, objectSerialNumber));
				if (children.Length == 0)
				{
					requestStatus = StatusOfTheRequest.NoRecordFound;
				}
			}

#if DEBUG
			var awbDebug = consignment as ICcsukCusAwb;
			if (awbDebug != null && awbDebug.CargoTerminalOperatorAirport == "XXX")
			{
				throw new NotFiniteNumberException("This is to blow up for testing");
			}
#endif
		}

		void FindChildrenAndGrandChildrenOfConsol_Export(IBusiness consignment, out IBusiness[] children)
		{
			var declarations = new List<JobDeclaration>();
			var consol = consignment as ForwardingConsol;
			if (consol != null)
			{
				foreach (var shipment in (from ForwardingShipment s in consol.Shipments where s.IsExport() select s))
				{
					foreach (var dec in (from JobDeclaration d in shipment.Declarations.OfType<JobDeclaration>() where d.IsExport && d.CountryCode == Core.Constants.CountryCodes.UnitedKingdom select d))
					{
						declarations.Add(dec);
					}
				}
			}
			children = declarations.ToArray();
		}

		internal static void FindChildrenAndGrandChildrenOfMawbOrBasic_Import(IBusiness consignment, out IBusiness[] children)
		{
			var iAwbChildren = new List<ICcsukCusAwb>();
			children = Array.Empty<IBusiness>();
			var mawbOrBasic = consignment as CusMAWB;
			var house = consignment as CusHAWB;
			if (mawbOrBasic != null)
			{
				if (mawbOrBasic.IsBasic)
				{
					if (mawbOrBasic.HasSplits)
					{
						foreach (SplitConsignment split in mawbOrBasic.Splits)
						{
							iAwbChildren.Add(split);
						}
					}
				}
				else
				{
					foreach (CusHAWB hawb in mawbOrBasic.ChildBills)
					{
						iAwbChildren.Add(hawb);
						if (hawb.HasSplits)
						{
							foreach (SplitHouse splitHouse in hawb.Splits)
							{
								iAwbChildren.Add(splitHouse);
							}
						}
					}
				}
			}
			else if (house != null)
			{
				foreach (SplitHouse split in house.Splits)
				{
					iAwbChildren.Add(split);
				}
			}
			iAwbChildren.Sort(new ICcsukCusAwbAscendingOrderComparer());
			children = iAwbChildren.ToArray();
		}

		void PrepareResponseMessages()
		{
			LucalGenralResponseMaker.New(requestStatus, inboundMessage, lucasQueryString, consignment, children, objectRequestType, commonAccessReference).MakeAndSendAllResponses();
		}

		void UpdateInboundMessage()
		{
			inboundMessage.EM_Status = EDIMessage.Status.Received;
		}

		internal enum EnquiryObjectTypes
		{
			MAWB,
			HAWB,
			MUCR,
			DUCR,
			SHPR,
			CNSE
		}

		internal enum StatusOfTheRequest
		{
			OK,
			BadRequest,
			NoRecordFound,
			InternalError
		}

		class ICcsukCusAwbAscendingOrderComparer : IComparer<ICcsukCusAwb>
		{
			public int Compare(ICcsukCusAwb awb1, ICcsukCusAwb awb2)
			{
				if (awb1.ReferenceNumber > awb2.ReferenceNumber)
				{
					return 1;
				}
				else if (awb1.ReferenceNumber < awb2.ReferenceNumber)
				{
					return -1;
				}
				else
				{
					return 0;
				}
			}
		}

		EnquiryObjectTypes objectRequestType;
		ZString objectSerialNumber;
		StatusOfTheRequest requestStatus = StatusOfTheRequest.OK;
		IBusiness consignment;
		IBusiness[] children;
		readonly ZString lucasQueryString;
		readonly GenralMessage genral;
		readonly EDIMessage inboundMessage;
		ZString commonAccessReference;
	}
}
