using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineIntegration;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class B3ImportDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		public B3ImportDocumentWrapper(B3Message b3Message)
			: base(FactoryExtensions.GetFactoryFromBusinessObject(b3Message))
		{
			B3Header = new B3AsLodgedDocumentWrapper(b3Message);
			Declaration = ((CusEntryHeader)((IEDIFACTMessageAttachee)b3Message.EM_LinkedObject)).Declaration;
		}

		public B3ImportDocumentWrapper(CusEntryHeader entryHeader, MessageSubTypes messageSubType = MessageSubTypes.Create)
			: base(FactoryExtensions.GetFactoryFromBusinessObject(entryHeader))
		{
			Argument.NotNull(entryHeader.Declaration, "businessObject.Declaration");
			Declaration = entryHeader.Declaration;
			B3Header = IsConsolidatedLVS ? new LowValueShipmentsMessageWrapper(entryHeader, messageSubType) : new B3ImportMessageWrapper(entryHeader, true);
			this.messageSubType = messageSubType;
		}

		public ZString TransactionNumberFormated
		{
			get { return string.Format("{0} - {1}", B3Header.AccountSecurityCode, B3Header.TransactionNumber); }
		}

		JobDeclaration Declaration
		{
			get => declaration;
			set
			{
				declaration = value;
				if (declaration != null)
				{
					IsConsolidatedLVS = declaration.IsConsolidatedLVS;
					IsLVX = declaration.IsLVX;
				}
			}
		}
		JobDeclaration declaration;

		public bool IsLVX { get; private set; }
		public bool IsConsolidatedLVS { get; private set; }

		public bool HasConsolidatedToLVS
		{
			get { return IsLVX && Declaration.Invoices[0].AdditionalDeclarations.Any(); }
		}

		public ZString JobNumber
		{
			get { return Declaration?.JE_DeclarationReference ?? ZString.Empty; }
		}

		public ZString ContainerNumber
		{
			get
			{
				if (!_containerNumber.HasValue)
				{
					var containerNumbers = Declaration?.CusContainers.Select(x => x.CO_ContainerNumber).OrderBy(x => x).ToArray();
					var noOfContainers = containerNumbers?.Length ?? 0;
					if (noOfContainers > 0)
					{
						if (noOfContainers > 4)
						{
							_containerNumber = string.Join(", ", containerNumbers.Take(3)) + ContainersNotPrinted(noOfContainers - 3);
						}
						else
						{
							_containerNumber = string.Join(", ", containerNumbers.Take(4));
						}
					}
					else
					{
						_containerNumber = ZString.Empty;
					}
				}
				return _containerNumber.Value;
			}
		}
		ZString? _containerNumber;

		public ZString OtherReferences => IsLVX ? Declaration.Invoices[0].CA_OtherReference : ZString.Empty;

		public ZString CarrierDescription
		{
			get
			{
				if (!_carrierDescription.HasValue)
				{
					if (!IsConsolidatedLVS && Declaration != null)
					{
						var carrierCode = IsLVX ? Declaration.LVXInvoiceHeader.CA_LVSCarrier : Declaration.JE_CarrierCode;
						if (!carrierCode.IsEmpty)
						{
							var zzRefCarrierCombined = new Universal.ZZRefCarrierCombined.Loader(Factory).LoadFromCode(Enterprise.Core.Constants.CountryCodes.Canada, carrierCode);
							_carrierDescription = zzRefCarrierCombined?.ZZ4_Description ?? ZString.Empty;
						}
						else
						{
							_carrierDescription = ZString.Empty;
						}
					}
				}
				return _carrierDescription.Value;
			}
		}
		ZString? _carrierDescription;

		public ZString ImporterRefNo
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration != null)
				{
					result = IsLVX ? Declaration.LVXInvoiceHeader.CA_OtherReference : Declaration.JE_OwnerRef;
				}
				return result;
			}
		}

		public ZString Vessel
		{
			get
			{
				var result = new ZStringBuilder();
				if (Declaration != null)
				{
					result.AppendIfNotEmpty(Declaration.JE_VesselName);
					result.AppendIfNotEmpty(Declaration.JE_VoyageFlightNo);
				}
				return result.ToStringWithDelimiterBetweenAppends("/");
			}
		}

		public ZString BillOfLading
		{
			get { return Declaration?.JE_MasterBill ?? ZString.Empty; }
		}

		string ContainersNotPrinted(int noOfContainersNoPrinted)
		{
			return Res.GetString("FD4F463A-6841-4657-A79F-DF6D28D17DB7", @", AND {0} MORE CONTAINERS", noOfContainersNoPrinted);
		}

		public ZDateTime CarrierDate
		{
			get { return IsLVX ? Declaration.LVXInvoiceHeader.JZ_InvoiceDate : ZDateTime.Empty; }
		}

		public ZString ClientCode
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration != null)
				{
					result = (IsLVX ? Declaration.LVXInvoiceHeader.Buyer?.OH_Code : Declaration.Importer?.OH_Code) ?? ZString.Empty;
				}
				return result;
			}
		}

		public ZString PrintedBy
		{
			get { return Env.CurrentUser.IsBatchProcessor ? Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, Declaration.JE_GS_NKCusAgent))?.GS_FullName ?? ZString.Empty : (ZString)Env.CurrentUser.LoginName; }
		}

		#region B3ImportDocumentPages

		public BusinessObjectCollectionWrapper<B3ImportDocumentPage> DocumentPages
		{
			get
			{
				return _documentPages ?? (_documentPages = new BusinessObjectCollectionWrapper<B3ImportDocumentPage>(B3ImportDocumentPages));
			}
		}
		BusinessObjectCollectionWrapper<B3ImportDocumentPage> _documentPages;

		public IEnumerable<B3ImportDocumentPage> B3ImportDocumentPages
		{
			get
			{
				if (b3ImportDocumentPages == null)
				{
					b3ImportDocumentPages = GetDocumentPages();
				}
				return b3ImportDocumentPages;
			}
		}
		IEnumerable<B3ImportDocumentPage> b3ImportDocumentPages;

		IEnumerable<B3ImportDocumentPage> GetDocumentPages()
		{
			var result = new List<B3ImportDocumentPage>();
			foreach (var b3SubHeader in B3Header.PositiveB3SubHeaders)
			{
				var page = new B3ImportDocumentPage(b3SubHeader);
				if (result.Count == 0)
				{
					page.AddHeader(B3Header);
				}

				foreach (var line1 in b3SubHeader.GetB3SubHeaderLines())
				{
					var firstLine = true;
					foreach (var line2 in line1.ClassificationLines)
					{
						AddLineToPage(result, ref page, new ClassificationLine(firstLine ? line1 : null, line2, b3SubHeader));
						firstLine = false;
					}
					if (firstLine)
					{
						AddLineToPage(result, ref page, new ClassificationLine(line1, null, b3SubHeader));
					}
				}
				if (!page.IsEmpty)
				{
					result.Add(page);
				}
			}

			var lastResult = result.LastOrDefault();
			if (lastResult != null)
			{
				lastResult.AddFooter(B3Header);
			}
			return result;
		}

		static void AddLineToPage(ICollection<B3ImportDocumentPage> pages, ref B3ImportDocumentPage page, ClassificationLine line)
		{
			if (!page.CanAddLine)
			{
				pages.Add(page);
				page = new B3ImportDocumentPage(null);
			}
			page.AddLine(line);
		}

		#endregion

		#region B3BInputReleases

		public ZBool HideCargoControlContinuationSheet
		{
			get { return B3Header.B3BInputReleases.Count() < 2; }
		}

		public BusinessObjectCollectionWrapper<B3BReleaseLine> B3BInputReleases
		{
			get
			{
				return _b3BInputReleases ?? (_b3BInputReleases = new BusinessObjectCollectionWrapper<B3BReleaseLine>(GetReleaseLine()));
			}
		}
		BusinessObjectCollectionWrapper<B3BReleaseLine> _b3BInputReleases;

		IEnumerable<B3BReleaseLine> GetReleaseLine()
		{
			var b3InputReleases = B3Header.B3BInputReleases.ToList();
			var linesCount = ((b3InputReleases.Count - 1) / 50 + 1) * 25;
			var result = new B3BReleaseLine[linesCount];
			var lineNum = 0;
			var index = 0;

			foreach (var b3BRelease in b3InputReleases)
			{
				index = (lineNum / 50) * 25 + lineNum % 25;
				if (result[index] == null)
				{
					result[index] = new B3BReleaseLine();
				}

				result[index].SetCCN(b3BRelease.CargoControlNumber, lineNum++);
			}

			if (result[index].CCN2.IsEmpty) // last CNN was set into first column, so there are some null lines which should be set
			{
				var number = result[index].Number;
				for (var i = index + 1; i < linesCount; i++)
				{
					result[i] = new B3BReleaseLine { Number = ++number };
				}
			}
			return result;
		}

		#endregion

		internal readonly IB3Header B3Header;
		internal readonly MessageSubTypes messageSubType;

		#region ISourceIdentifierProvider members

		ZGuid ISourceIdentifierProvider.SourceIdentifier => Declaration?.PK ?? ZGuid.Empty;

		#endregion
	}

	#region Additional Classes

	#region B3BReleaseLine

	class B3BReleaseLine : NonPersistentBusinessObject
	{
		public ZInt Number { get; internal set; }
		public ZString CCN1 { get; private set; }
		public ZString CCN2 { get; private set; }

		internal void SetCCN(ZString ccn, int lineNum)
		{
			if (lineNum / 25 % 2 == 0)
			{
				CCN1 = ccn;
				Number = lineNum + 1;
			}
			else
			{
				CCN2 = ccn;
			}
		}
	}

	#endregion

	#region B3Footer

	class B3Footer : NonPersistentBusinessObject
	{
		public B3Footer(IB3Header header)
		{
			this.header = header;
		}

		public ZString WarehouseNumber
		{
			get { return header.WarehouseNumber; }
		}

		public ZString CarrierCodeAtImportation
		{
			get { return header.CarrierCodeAtImportation; }
		}

		public ZString CargoControlNumber
		{
			get { return header.B3BInputReleases.Count() > 1 ? new ZString("B3B") : !header.B3BInputReleases.Any() ? ZString.Empty : header.B3BInputReleases.First().CargoControlNumber; }
		}

		public ZDateTime CurrentDate
		{
			get { return ZDateTime.Now; }
		}

		#region BrokerNameAndPhone

		public ZString BrokerNameAndPhone
		{
			get { return Helper.GetCusAgentNameAndPhone(header.TopLevelBusinessObject) ?? Helper.GetCurrentUserNameAndPhone(); }
		}

		#endregion

		public Image BrokerSignatureImage
		{
			get
			{
				Image image = null;
				var declaration = header.TopLevelBusinessObject as JobDeclaration;
				if (declaration != null && declaration.IsPrintBrokerSignatureImage)
				{
					var broker = declaration.DeclarantOnEntryDocsBrokerOnB3 ?? declaration.CusAgent;
					if (broker != null)
					{
						image = broker.SignatureImage;
					}
				}
				return image;
			}
		}

		#region Totals

		public ITotalAmounts Totals
		{
			get { return !IsTotalAmountsEmpty(header.PositiveTotalAmounts) ? header.PositiveTotalAmounts : header.NegativeTotalAmounts; }
		}

		protected static bool IsTotalAmountsEmpty(ITotalAmounts totalAmounts)
		{
			return totalAmounts.Deposit.IsEmpty
						 && totalAmounts.TotalAllDutyAndTaxes.IsEmpty
						 && totalAmounts.TotalCustomsDuty.IsEmpty
						 && totalAmounts.TotalExciseTax.IsEmpty
						 && totalAmounts.TotalGST.IsEmpty
						 && totalAmounts.TotalSIMAAssessment.IsEmpty;
		}

		#endregion

		readonly IB3Header header;

		B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
		B3AndCADDocumentHelper helper;
	}

	#endregion

	#region BusinessObjectCollectionWrapper

	public sealed class BusinessObjectCollectionWrapper<T> : NonPersistentBusinessObjectCollection<T> where T : NonPersistentBusinessObject
	{
		public BusinessObjectCollectionWrapper(IEnumerable<T> collection)
		{
			foreach (var obj in collection)
			{
				if (obj != null)
				{
					Add(obj);
				}
			}
		}

		#region Overrides of NonPersistentBusinessObjectCollection

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}

	#endregion

	#endregion
}
