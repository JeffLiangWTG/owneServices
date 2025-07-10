using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CH.Business;

public class ExportJobDeclarationValidation : JobDeclarationValidation
{
	public ExportJobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	protected override void CheckJE_GoodsDestination()
	{
		base.CheckJE_GoodsDestination();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_GoodsDestinationInfo);

		if (!Parent.IsExportDeclarationActivation)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsDestinationInfo);
		}
	}

	protected override void CheckJE_OH_Supplier()
	{
		base.CheckJE_OH_Supplier();

		var targetPropertyInfo = Parent.JE_OH_SupplierInfo;
		if (!Parent.IsExportActivationPassar)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}

		var supplierAddress = Parent.Supplier?.MainAddress;
		PlausiValidation.CheckNP70172(targetPropertyInfo, supplierAddress, DocAddressType.SupplierDocumentaryAddress);
	}

	protected override void CheckJE_OH_Importer()
	{
		base.CheckJE_OH_Importer();

		if (Parent.IsExport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ImporterInfo);
		}
	}

	protected override void CheckDeclarationNumber()
	{
		base.CheckDeclarationNumber();

		if (Parent.IsExportDeclarationActivation && (Parent.JE_MessageSubType == ActivationTypeList.Codes.Edec || Parent.JE_MessageSubType == ActivationTypeList.Codes.Passar))
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DeclarationNumberInfo);

			if (!Parent.DeclarationNumber.IsEmpty)
			{
				if (Parent.JE_MessageSubType == ActivationTypeList.Codes.Edec && !Regex.IsMatch(Parent.DeclarationNumber, @"^[0-9]{2}CHEE[A-Z,0-9]{10}[A-M,O-Z,0-9]{1}[A-Z,0-9]{1}\.[0-9]+"))
				{
					Parent.DeclarationNumberInfo.AddMessageError(Res.GetString("45913A71-5DA5-4F42-9EC7-1F0ECC6F9DBF", "Wrong eDec format. It should be {0} like {1}", "nnCHEEnnnnnnnnnnnc.n", "25CHEE012345678901.1"));
				}
				else if (Parent.JE_MessageSubType == ActivationTypeList.Codes.Passar && !Regex.IsMatch(Parent.DeclarationNumber, @"^[0-9]{2}CH[A-D,F-Z,0-9]{2}[A-Z,0-9]{10}[N]{1}[A-Z,0-9]{1}\.[0-9]+"))
				{
					Parent.DeclarationNumberInfo.AddMessageError(Res.GetString("0BCDA1CA-AE46-48D3-806D-AE6CFFEA94C7", "Wrong Passar format. It should be {0} like {1}", "nnCHxxxxxxxxxxxxNc.nn", "25CH01EXCABC1234N1.1"));
				}
				if (Regex.IsMatch(Parent.DeclarationNumber, @".{18}\.\d+"))
				{
					var otherEntryHeader = new CusEntryHeader.Loader(Parent.Factory).FindByEntryNumber(Parent.DeclarationNumber, anyVersion: true, jobMessageType: CHJobMessageTypeList.Codes.ExportDeclarationActivation, excludeJobDeclarationPK: Parent.PK);
					if (otherEntryHeader != null)
					{
						Parent.DeclarationNumberInfo.AddWarning(Res.GetString("BD341B64-539B-40C2-9E90-B1875CDB5EAB", "The entered GDRN is already used in another EDA job: {0}.", otherEntryHeader.Declaration.JobNumber));
					}
				}
			}
		}
	}

	protected override void CheckJE_MessageSubType()
	{
		base.CheckJE_MessageSubType();
		if (Parent.IsExportDeclarationActivation)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_MessageSubTypeInfo);
		}
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		if (Parent.IsExportActivationPassar)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_LocationOfGoodsInfo);
		}
	}

	protected override void CheckJE_VesselName()
	{
		base.CheckJE_VesselName();

		if (Parent.IsExportActivationPassar
			&& ((!Parent.JE_TransportMode.IsEmpty
			&& !Parent.JE_RN_NKTransportNationality.IsEmpty
			&& !Parent.IsAir)
			|| (!Parent.JE_TransportMeans.IsEmpty
			&& Parent.JE_TransportMode == TransportTypeList.Codes.OwnPropulsion)))
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo);
		}
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();

		if (Parent.IsExportActivationPassar
			&& !Parent.JE_TransportMode.IsEmpty
			&& ((!Parent.IsAir
			&& !Parent.JE_VesselName.IsEmpty)
			|| (Parent.IsAir
			&& !Parent.JE_VoyageFlightNo.IsEmpty)))
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
		}
	}

	protected override void CheckJE_TransportMeans()
	{
		base.CheckJE_TransportMeans();

		if (Parent.IsExportActivationPassar && Parent.JE_TransportMode == TransportTypeList.Codes.OwnPropulsion)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportMeansInfo);
		}
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();
		if (Parent.IsExportDeclarationActivation && Parent.JE_MessageSubType == ActivationTypeList.Codes.Edec)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CustomsOfficeInfo);
		}
	}

	protected override void CheckJE_TransportModeMandatory()
	{
	}

	protected override void CheckJE_DeclarationLanguage()
	{
		if (!Parent.IsExportDeclarationActivation)
		{
			base.CheckJE_DeclarationLanguage();
		}
	}

	protected override void CheckJE_VoyageFlightNo()
	{
		base.CheckJE_VoyageFlightNo();

		if (Parent.IsExportActivationPassar
			&& !Parent.JE_TransportMode.IsEmpty
			&& !Parent.JE_RN_NKTransportNationality.IsEmpty
			&& Parent.IsAir)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo);
		}
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OA_DeclarantAddressInfo);
		var declarant = (OrgHeader)Parent.JE_OA_DeclarantAddress_ZAddress.OrgHeader;
		PlausiValidation.CheckNP70127(Parent.JE_OA_DeclarantAddressInfo, declarant);
		PlausiValidation.CheckNS30117(Parent.JE_OA_DeclarantAddressInfo, Parent);
	}

	protected override void CheckJE_GS_NKCusAgent()
	{
		base.CheckJE_GS_NKCusAgent();
		if (!Parent.IsExport)
		{
			CheckJE_GS_NKCusAgent_Password();
		}
	}
}
