using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ORtfTextUtil = Enterprise.ZArchitecture.Core.ORtfTextUtil;

namespace Enterprise.Client.Wow
{
	public class OrderCsvRecord : CsvRecord
	{
		public OrderCsvRecord(string line) : base(line, 15)
		{
			OrderNumber = FieldValues[1].Trim();
			PortOfLoading = FieldValues[14].Trim();
			IncoTerm = FieldValues[10].Trim();
			OperationCode = char.ToUpper(FieldValues[2][0]);
		}

		public readonly ZString OrderNumber;
		public readonly ZString PortOfLoading;
		public readonly ZString IncoTerm;
		public readonly char OperationCode;

		public override string DisplayIdentifier
		{
			get { return "Order " + OrderNumber; }
		}

		protected override void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			if (OperationCode == 'D')
			{
				WoolworthsOrder order = GetOrCreateOrder(factoryProvider.Current);
				if (order != null)
				{
					order.Delete();
				}
			}
			else
			{
				Order bO = GetOrCreateOrder(factoryProvider.Current);
				if (bO != null)
				{
					((ISupportDataImporting)bO).IsImportingData = true;
					try
					{
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.JD_OrderNumberInfo, FieldValues[1], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.SupplierPKInfo, FieldValues[4], ForeignKeyType.OrganisationMatchWithFullName, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.JD_CustomAttrib1Info, FieldValues[5], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.BuyerPKInfo, FieldValues[7], ForeignKeyType.OrganisationMatchWithFullName, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.JD_RX_NKOrderCurrencyInfo, FieldValues[6], ForeignKeyType.CurrencyNK, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.JD_IncoTermInfo, FieldValues[10], ForeignKeyType.None, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.JD_FirstBuyerContactInfo, FieldValues[12], ForeignKeyType.Contact, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.JD_SecondBuyerContactInfo, FieldValues[13], ForeignKeyType.Contact, notify);
						WowStringToBusinessObjectFieldConverter.Instance.SetPropertyInfoValue(
							bO.JD_RL_NKPortOfLoadingInfo, FieldValues[14], ForeignKeyType.PortNK, notify);
					}
					finally
					{
						((ISupportDataImporting)bO).IsImportingData = false;
					}

					UpdateIncoTermToNotes(bO);
				}
			}
		}

		#region Implementation

		#region Column Mappings

		internal static readonly string[] OrderPropertyMappings = new string[]
		{
			"",									// Record type
			Order.Schema.JD_OrderNumber,		// Order Number
			"",									// Order Control Indicator: 'A' add, 'U' update, 'D' Delete
			"",									// Supplier ID - Master File
			Order.Schema.SupplierPK,			// Supplier Name - Master file (Code Matching Only)
			Order.Schema.JD_CustomAttrib1,		// ediTrack
			Order.Schema.JD_RX_NKOrderCurrency,	// Currency
			Order.Schema.BuyerPK,				// Match to Unknown Brand
			"",									// Departure Port ID
			"",									// Delivery Term Code
			Order.Schema.JD_IncoTerm,			// Buying Term Code
			"",									// Additional Delivery Term
			Order.Schema.JD_FirstBuyerContact,	// First Buyer Contact
			Order.Schema.JD_SecondBuyerContact, // Second Buyer Contact
			Order.Schema.JD_RL_NKPortOfLoading, // Port of Loading
		};

		#endregion

		protected WoolworthsOrder GetOrCreateOrder(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, OrderNumber);
			WoolworthsOrder result = (WoolworthsOrder)factory.LoadTop1(typeof(WoolworthsOrder), filter);
			if (result == null)
			{
				result = (WoolworthsOrder)factory.New(typeof(WoolworthsOrder));
				result.BuyerPK = WowDataRegistry.Instance.UnmatchedDataItemsAccount;
				RefServiceLevel serviceLevel = (RefServiceLevel)factory.LoadTop1(
					typeof(RefServiceLevel),
					new ZQuery(RefServiceLevelSchema.RS_Code, SQLComparisonOperator.Equal, WowConstants.StandardServiceLevelCode));
				result.JD_RS_NKServiceLevel_NI = serviceLevel.RS_Code;
				result.JD_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			}
			return result;
		}

		protected void UpdateIncoTermToNotes(Order bO)
		{
			StmNote incoTermNote = null;
			string newDataRtf = ORtfTextUtil.TextToRtf(IncoTerm);
			ZBlob newNoteData = ZBlob.FromAscii(newDataRtf);
			foreach (StmNote note in bO.Notes.GetAllNotes())
			{
				if (note.ST_Description.Trim().ToLower().Equals(WowConstants.OrderIncoTermNoteDescription.ToLower()))
				{
					incoTermNote = note;
				}
			}
			if (incoTermNote == null)
			{
				incoTermNote = bO.Notes.AddNew();
				incoTermNote.ST_IsCustomDescription = true;
				incoTermNote.ST_Description = WowConstants.OrderIncoTermNoteDescription;
			}
			if (!newNoteData.Equals(incoTermNote.ST_NoteData))
			{
				incoTermNote.ST_NoteData = newNoteData;
			}
		}

		#endregion
	}
}
