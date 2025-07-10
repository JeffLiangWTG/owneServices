using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPRLCusTempStorageLineValidation : CusTempStorageLineValidation
	{
		public CUSPRLCusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new CUSPRLCusTempStorageLine Parent => (CUSPRLCusTempStorageLine)base.Parent;

		protected override void CheckTSL_UnionStatus()
		{
			base.CheckTSL_UnionStatus();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TSL_UnionStatusInfo);
		}

		protected override void CheckTSL_GrossWeight()
		{
			base.CheckTSL_GrossWeight();
			MandatoryValidation.MessageErrorIfIsZero(Parent.TSL_GrossWeightInfo);
			if (Parent.TSL_GrossWeight > 99999999999.999m)
			{
				Parent.TSL_GrossWeightInfo.AddMessageError(Res.GetString("406E1BE0-4A44-4772-AEA4-66B633518B57", "Maximum value allowed is 99999999999.999"));
			}
		}

		protected override void CheckTSL_PackageQty()
		{
			base.CheckTSL_PackageQty();
			CheckPackageQtyIsBetween1And99999();
			CheckSinglePackageRequirements();
		}

		protected override void CheckTSL_RN_NKDepartureCountry()
		{
			base.CheckTSL_RN_NKDepartureCountry();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_RN_NKDepartureCountryInfo);
		}

		protected override void CheckTSL_GoodsDescription()
		{
			base.CheckTSL_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_GoodsDescriptionInfo);
		}

		protected override void CheckTSL_OwnerReferenceNumber()
		{
			base.CheckTSL_OwnerReferenceNumber();
			if (!Parent.TSL_OwnerReferenceType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceNumberInfo);
			}
		}

		protected override void CheckTSL_ReferenceNumber()
		{
			base.CheckTSL_ReferenceNumber();
			CheckReferenceNumber(Parent.TSL_ReferenceNumber, Parent.TSL_ReferenceNumberInfo, Res.GetString("99d35552-882f-46b0-ba54-4d9b5733c5ed", "When SumA previous reference type is 'ESUMA', 'ENST2L' or 'N355',"), PreviousReferenceType.Codes._ESUMA, PreviousReferenceType.Codes._ENST2L, PreviousReferenceType.Codes._N355);
		}

		protected override void CheckTSL_ReferenceNumberLine()
		{
			base.CheckTSL_ReferenceNumberLine();
			if (IsStorageHeaderReferenceTypeEqualTo(PreviousReferenceType.Codes._ESUMA, PreviousReferenceType.Codes._ENST2L))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_ReferenceNumberLineInfo);
			}
		}

		protected override void CheckTSL_CustodianIdentifierBranchNoMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_CustodianIdentifierBranchNoInfo);
		}

		protected override void CheckTSL_GoodsOwnerIdentifierBranchNoMandatory()
		{
			//No warning or message error for not entering DET Branch at CUSPRL
		}

		protected override void CheckTSL_LineNo()
		{
			base.CheckTSL_LineNo();
			var storageDec = Parent.Dec;
			if (storageDec != null && storageDec.CusTempStorageLines.Cast<CUSPRLCusTempStorageLine>().Any(x => x.TSL_LineNo == Parent.TSL_LineNo && x.PK != Parent.PK))
			{
				Parent.TSL_LineNoInfo.AddMessageError(Res.GetString("82C906D4-2F3F-4944-9C77-AE4CBCF27AC2", "Line No. {0} already exists for this declaration", Parent.TSL_LineNo));
			}
		}

		protected override void CheckTSL_PackageType()
		{
			base.CheckTSL_PackageType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_PackageTypeInfo);
		}

		protected override void CheckTSL_ReferenceNumber2()
		{
			base.CheckTSL_ReferenceNumber2();
			CheckReferenceNumber(Parent.TSL_ReferenceNumber2, Parent.TSL_ReferenceNumber2Info, Res.GetString("360d0cff-f693-4a6d-8644-0f412c8650ff", "When SumA previous reference type is 'ENST2L', 'POUS',"), PreviousReferenceType.Codes._ENST2L, PreviousReferenceType.Codes._POUS);
		}

		protected override void CheckTSL_ReferenceNumber2Line()
		{
			base.CheckTSL_ReferenceNumber2Line();
			if (IsStorageHeaderReferenceTypeEqualTo(PreviousReferenceType.Codes._ENST2L, PreviousReferenceType.Codes._POUS))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_ReferenceNumber2LineInfo);
			}
		}

		protected override void CheckTSL_TransportNumberType()
		{
			base.CheckTSL_TransportNumberType();

			var propertyInfo = Parent.TSL_TransportNumberTypeInfo;
			if (IsStorageHeaderReferenceTypeEqualTo(PreviousReferenceType.Codes._N355)
				&& !Parent.TSL_TransportNumber.IsEmpty && Parent.Receptacle.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(propertyInfo);
		}

		protected override void CheckTSL_TransportNumber()
		{
			base.CheckTSL_TransportNumber();

			if (!Parent.TSL_TransportNumberType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_TransportNumberInfo);
			}
		}

		bool IsStorageHeaderReferenceTypeEqualTo(params ZString[] referenceTypes) => referenceTypes.Contains(Parent.StorageHeader?.SJH_PreviousReferenceType ?? ZString.Empty);

		void CheckReferenceNumber(ZString referenceNumber, ZPropertyInfo referenceNumberInfo, ZString mrmformatPrefixMessage, params ZString[] headerReferenceTypes)
		{
			if (IsStorageHeaderReferenceTypeEqualTo(headerReferenceTypes))
			{
				var previousReferenceNumber = Parent.StorageHeader.SJH_PreviousReferenceNumber;
				if (previousReferenceNumber.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(referenceNumberInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(referenceNumberInfo);
				}
				if (!referenceNumber.IsEmpty)
				{
					var mrnError = MRNFormatValidator.CheckMRNFormat(referenceNumber, Parent.Factory, mrmformatPrefixMessage);
					if (!mrnError.IsEmpty)
					{
						referenceNumberInfo.AddMessageError(mrnError);
					}
				}
			}
		}
	}
}
