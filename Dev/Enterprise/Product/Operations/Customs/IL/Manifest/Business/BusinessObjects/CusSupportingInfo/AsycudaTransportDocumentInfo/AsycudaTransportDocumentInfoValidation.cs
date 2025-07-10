using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaTransportDocumentInfoValidation : CusSupportingInfoValidation
	{
		public AsycudaTransportDocumentInfoValidation(AutoCusSupportingInfo parent) : base(parent)
		{
			zValidationInternals = this;
		}

		public new AsycudaTransportDocumentInfo Parent => (AsycudaTransportDocumentInfo)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateCSI_CodeUserInterface();
			ValidateCSI_ReferenceNumberUserInterface();
		}

		public void ValidateCSI_CodeUserInterface()
		{
			zValidationInternals.Validate(Parent.CSI_CodeUserInterfaceInfo, GetCSI_CodeUserInterfaceValidationInvoker());
		}

		public void ValidateCSI_ReferenceNumberUserInterface()
		{
			zValidationInternals.Validate(Parent.CSI_ReferenceNumberUserInterfaceInfo, GetCSI_ReferenceNumberUserInterfaceValidationInvoker());
		}

		public void CheckCSI_CodeUserInterface()
		{
			CheckRuleCC_BR1_WCO_090();
			CheckDuplications();
		}

		public void CheckCSI_ReferenceNumberUserInterface()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberUserInterfaceInfo);
			CheckRuleCC_BR1_WCO_091();
			CheckReferenceNumberUniqueIL1ForwarderDeal();
			CheckReferenceNumber_First3DigitsMatchForIL1AndIL2();
		}

		RunValidationInvoker GetCSI_CodeUserInterfaceValidationInvoker()
		{
			return delegate
			{
				CheckCSI_CodeUserInterfaceInfoIsWesternEuropean();
				CheckCSI_CodeUserInterface();
			};
		}

		RunValidationInvoker GetCSI_ReferenceNumberUserInterfaceValidationInvoker()
		{
			return delegate
			{
				CheckCSI_ReferenceNumberUserInterface();
			};
		}

		void CheckCSI_CodeUserInterfaceInfoIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.CSI_CodeUserInterfaceInfo);
		}

		void CheckReferenceNumberUniqueIL1ForwarderDeal()
		{
			var parent = Parent;
			if (parent.CSI_Code != TransportDocsTypeList.Codes.IL1
				|| (parent.CSI_ReferenceNumber is ZString uniqueIL1ForwarderDealReferenceNumber && uniqueIL1ForwarderDealReferenceNumber.IsEmpty))
			{
				return;
			}

			var header = parent.Parent.Header;
			if (!header.IsSea)
			{
				return;
			}

			var info = parent.CSI_ReferenceNumberUserInterfaceInfo;
			if (header.Bills.Cast<AsycudaBill>()
				.SelectMany(d => d.TransportDocuments)
				.Where(td => td != parent)
				.Any(td => td.CSI_ReferenceNumber == uniqueIL1ForwarderDealReferenceNumber))
			{
				info.AddError($"{ValidationCaptions.AsycudaTransportDocumentInfo.UniqueIL1ForwarderDealReferenceNumber} '{header.AMA_JobReference}'");
				return;
			}

			var manifestNumber = header.AMA_ManifestNumber;
			if (manifestNumber.IsEmpty)
			{
				return;
			}
			var differentHeaderWithSameManifestNumberAndIL1ForwarderDealNumber = FetchDifferentHeaderWithSameManifestNumberAndIL1ForwarderDealNumber(parent.Factory, manifestNumber, header.PK, uniqueIL1ForwarderDealReferenceNumber);
			if (differentHeaderWithSameManifestNumberAndIL1ForwarderDealNumber != null)
			{
				info.AddError($"{ValidationCaptions.AsycudaTransportDocumentInfo.UniqueIL1ForwarderDealReferenceNumber} '{differentHeaderWithSameManifestNumberAndIL1ForwarderDealNumber.AMA_JobReference}'");
			}
		}

		static AsycudaManifestHeader FetchDifferentHeaderWithSameManifestNumberAndIL1ForwarderDealNumber(BusinessObjectFactory factory, string manifestNumber, ZGuid headerPk, ZString uniqueIL1ForwarderDealReferenceNumber)
		{
			var csiSubQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, AsycudaBillSchema.Constants.Prefix);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_SubType, AdditionalInfoSubTypeList.Codes.TransportDocument);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_Code, TransportDocsTypeList.Codes.IL1);
			csiSubQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, uniqueIL1ForwarderDealReferenceNumber);

			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.PK, SQLComparisonOperator.NotEqual, headerPk);
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, CountryCodes.Israel);
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestNumber, manifestNumber);

			var billSubQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			billSubQuery.AddSubQuery(csiSubQuery, JoinCondition.And);
			headerQuery.AddSubQuery(billSubQuery, JoinCondition.And);

			return factory.LoadTop1<AsycudaManifestHeader>(headerQuery);
		}

		void CheckRuleCC_BR1_WCO_090()
		{
			AsycudaManifestHeader header = null;
			AsycudaBill bill = null;
			var parent = Parent;
			if (parent != null)
			{
				if (parent.Parent is AsycudaBill parentBill)
				{
					bill = parentBill;
					header = bill.Header;
				}
			}

			if (header != null)
			{
				if (parent.CSI_Code == TransportDocsTypeList.Codes.IL2)
				{
					if (header.AMA_TransportMode == Core.Constants.TransportModes.Sea && bill.TransportDocuments.All(s => s.CSI_Code != TransportDocsTypeList.Codes.IL1))
					{
						parent.CSI_CodeUserInterfaceInfo.AddMessageError(ValidationCaptions.AsycudaTransportDocumentInfo.RuleCC_BR1_WCO_090_IL1);
					}
				}
			}
		}

		void CheckRuleCC_BR1_WCO_091()
		{
			var parent = Parent;
			if (!parent.CSI_ReferenceNumber.IsEmpty)
			{
				if ((parent.CSI_Code == TransportDocsTypeList.Codes.IL1 || parent.CSI_Code == TransportDocsTypeList.Codes.IL2) &&
					!Regex.IsMatch(parent.CSI_ReferenceNumber, Constants.AsycudaTransportDocumentInfoValidation.RuleCC_BR1_WCO_091_IL1IL2_Regex))
				{
					parent.CSI_ReferenceNumberUserInterfaceInfo.AddMessageError(ValidationCaptions.AsycudaTransportDocumentInfo.RuleCC_BR1_WCO_091_IL1IL2);
				}
				if (parent.CSI_Code == TransportDocsTypeList.Codes.IL3 &&
					!Regex.IsMatch(parent.CSI_ReferenceNumber, Constants.AsycudaTransportDocumentInfoValidation.RuleCC_BR1_WCO_091_IL3_Regex))
				{
					parent.CSI_ReferenceNumberUserInterfaceInfo.AddMessageError(ValidationCaptions.AsycudaTransportDocumentInfo.RuleCC_BR1_WCO_091_IL3);
				}
			}
		}

		void CheckDuplications()
		{
			var parent = Parent;
			var bill = parent.Parent;

			if (bill.TransportDocuments.Any(transportDocument => transportDocument != parent && transportDocument.CSI_Code == parent.CSI_Code))
			{
				parent.CSI_CodeUserInterfaceInfo.AddError(ValidationCaptions.AsycudaTransportDocumentInfo.RuleTypeDuplication);
			}
		}

		void CheckReferenceNumber_First3DigitsMatchForIL1AndIL2()
		{
			var parent = Parent;
			if (parent.CSI_Code != TransportDocsTypeList.Codes.IL1 && parent.CSI_Code != TransportDocsTypeList.Codes.IL2)
			{
				return;
			}

			var transportDocs = parent.Parent.TransportDocuments;
			var transportDocumentIL1 = transportDocs.FirstOrDefault(x => x.CSI_Code == TransportDocsTypeList.Codes.IL1);
			var transportDocumentIL2 = transportDocs.FirstOrDefault(x => x.CSI_Code == TransportDocsTypeList.Codes.IL2);

			if (transportDocumentIL1 == null || transportDocumentIL2 == null)
			{
				return;
			}

			var referenceNumberIL1 = transportDocumentIL1.CSI_ReferenceNumber;
			var referenceNumberIL2 = transportDocumentIL2.CSI_ReferenceNumber;

			if (referenceNumberIL1.Length < 4 || referenceNumberIL2.Length < 4 || referenceNumberIL1.SubstringSafe(1, 3) != referenceNumberIL2.SubstringSafe(1, 3))
			{
				parent.CSI_ReferenceNumberUserInterfaceInfo.AddMessageError(ValidationCaptions.AsycudaTransportDocumentInfo.First3digitsDifferentIL1IL2);
			}
		}

		readonly IValidationInternals zValidationInternals;
	}
}
