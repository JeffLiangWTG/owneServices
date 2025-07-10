using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.Declaration
{
	sealed class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
		{
		}

		protected override void CheckCGL_Type()
		{
			base.CheckCGL_Type();
			if (IsUCC6)
			{
				MandatoryValidation.MessageErrorIfNotEntered(GoodsLocation.CGL_TypeInfo);
			}

			if (IsTemporaryStorageThatNeedsValidation)
			{
				CheckBR_PN_TS_FR03();
			}

			if (ParentObject is TemporaryStorageHeader header)
			{
				if (header.AMA_ManifestType == FRConstants.TemporaryStorage.AppCodeIST)
				{
					CheckBR_PN_TS_FR08();
				}
				else if (header.AMA_ManifestType == FRConstants.TemporaryStorage.AppCodeLAD)
				{
					CheckBR_PN_TS_FR06();
				}
			}
		}

		void CheckBR_PN_TS_FR06()
		{
			var parent = Parent;
			if (parent.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace)
			{
				parent.CGL_TypeInfo.AddWarning(Res.GetString("331D7CEB-C213-456D-AC06-4C75682A8E25", "Type of location should not be B when Temporary Storage is LAD."));
			}
		}

		void CheckBR_PN_TS_FR08()
		{
			var parent = Parent;
			if (parent.CGL_Type == CusGoodsLocationTypeList.Codes.ApprovedPlace)
			{
				parent.CGL_TypeInfo.AddWarning(Res.GetString("A65E7EED-D57E-4497-8F64-6756F88430E8", "Type of location should not be C when Temporary Storage is IST."));
			}
		}

		protected override void CheckCGL_Qualifier()
		{
			base.CheckCGL_Qualifier();

			if (IsTemporaryStorageThatNeedsValidation)
			{
				CheckBR_PN_TS_FR01();
				CheckBR_PN_TS_FR04();
			}
		}

		protected override void CheckCGL_AdditionalIdentifier()
		{
			base.CheckCGL_AdditionalIdentifier();
			if (Parent.Parent is TemporaryStorageHeader)
			{
				Check_BR_PN_TS_02();
			}
			if (Parent.Parent is CusEntryInstruction)
			{
				CheckCGL_AdditionalIdentifierFromEntryInstruction();
			}
		}

		void CheckCGL_AdditionalIdentifierFromEntryInstruction()
		{
			var parent = Parent;
			var identifier = parent.CGL_AdditionalIdentifier;
			var identifierInfo = parent.AdditionalIdentifierInfo;
			var r = new Regex("^[a-zA-Z0-9]{4}$");

			if ((parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier && parent.CGL_Type == CusGoodsLocationTypeList.Codes.DesignatedLocation) && (identifier.IsEmpty || !r.IsMatch(identifier)))
			{
				identifierInfo.AddMessageError(Res.GetString("E3A7F9D8-5C12-4A9F-B8E2-91D3F7A65B4C", "Additional Identifier requires a 4AN reference"));
			}
		}

		void CheckBR_PN_TS_FR01()
		{
			var parent = Parent;
			if (parent.CGL_Type == CusGoodsLocationTypeList.Codes.DesignatedLocation && parent.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier)
			{
				parent.CGL_QualifierInfo.AddMessageError(Res.GetString("08EDF72B-D642-42E6-A43A-1F891A7DCB92", "Qualifier of identification must be V when type is A."));
			}
		}

		void CheckBR_PN_TS_FR03()
		{
			var parent = Parent;
			if (parent.CGL_Type == CusGoodsLocationTypeList.Codes.Other)
			{
				parent.CGL_TypeInfo.AddMessageError(Res.GetString("5035BDB8-2E99-4D07-8484-33EE5A912DE7", "Type of location cannot be D."));
			}
		}

		void CheckBR_PN_TS_FR04()
		{
			var parent = Parent;
			var type = parent.CGL_Type.ToUpperInvariant();
			if ((type == CusGoodsLocationTypeList.Codes.AuthorizedPlace || type == CusGoodsLocationTypeList.Codes.ApprovedPlace) && parent.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.AuthorizationNumber)
			{
				parent.CGL_QualifierInfo.AddMessageError(Res.GetString("F624CA0E-10FD-4A33-9128-555B9A4AB72F", "Qualifier of identification must be Y when type is B or C."));
			}
		}

		void Check_BR_PN_TS_02()
		{
			var parent = Parent;
			var identifier = parent.CGL_AdditionalIdentifier;
			var identifierInfo = parent.AdditionalIdentifierInfo;
			var r = new Regex("^[a-zA-Z0-9]{1,4}$");

			if (parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier && parent.CGL_Type == CusGoodsLocationTypeList.Codes.DesignatedLocation)
			{
				if (identifier.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(identifierInfo);
				}
				else if (!r.IsMatch(identifier))
				{
					identifierInfo.AddMessageError(Res.GetString("4BF6CCB2-2A41-42F4-8ED7-694139CBEECD", "Additional Identifier must contain up to 4 alphanumeric characters."));
				}
			}
		}

		protected override bool IsQualifierInvalidForType(string type, string qualifier) => false;

		bool IsUCC6 => ParentObject is CusEntryInstruction entryInstruction && ((JobDeclaration)entryInstruction.JobDeclaration).IsUCC6;

		bool IsTemporaryStorageThatNeedsValidation => ParentObject is TemporaryStorageHeader storageHeader && (storageHeader.AMA_MessageType == PNTSMessageTypeList.Codes.PreLodgedTempStorage || storageHeader.AMA_MessageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage || storageHeader.AMA_MessageType == PNTSMessageTypeList.Codes.PresentationNotification);

		BusinessObject ParentObject => parentObject ?? (parentObject = Parent.Parent);
		BusinessObject parentObject;

		CusGoodsLocation GoodsLocation => (CusGoodsLocation)Parent;
	}
}
