using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

public class CGMAsycudaManifestHeaderValidation : ManifestHeaderValidation
{
	public CGMAsycudaManifestHeaderValidation(CGMAsycudaManifestHeader parent)
		: base(parent)
	{
	}

	public void ValidateImportGeneralManifestNumber()
	{
		ValidateCalculatedProperty(Parent.ImportGeneralManifestNumberInfo);
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateImportGeneralManifestNumber();
	}

	protected new CGMAsycudaManifestHeader Parent => (CGMAsycudaManifestHeader)base.Parent;

	protected override void CheckAMA_NatureCore()
	{
		base.CheckAMA_NatureCore();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_NatureInfo);
	}

	protected override void CheckRegistrationStatus()
	{
		base.CheckRegistrationStatus();
		ListValidation.ErrorIfInvalidCode(Parent.RegistrationStatusInfo);
	}

	protected void CheckImportGeneralManifestNumber()
	{
		var parent = Parent;
		if (!parent.ImportGeneralManifestNumber.IsNumbersOnlyOrEmpty)
		{
			var importGeneralManifestNumberInfo = parent.ImportGeneralManifestNumberInfo;
			importGeneralManifestNumberInfo.AddMessageError(Res.GetString("F65348CF-07D7-4A99-8D11-EC9403F9F86F", "You have entered an invalid {0}. The expected value is a {1}-digit number.", importGeneralManifestNumberInfo.HumanReadableName, importGeneralManifestNumberInfo.MaxLength));
		}
	}

	protected override void CheckAMA_RL_NKPortOfFirstArrival()
	{
		if (!Parent.IsSea)
		{
			base.CheckAMA_RL_NKPortOfFirstArrival();
		}
	}

	protected override void CheckAMA_VesselName()
	{
		base.CheckAMA_VesselName();
		if (Parent.IsSea)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_VesselNameInfo);
		}
	}

	protected override void CheckAMA_CustomsOffice()
	{
		base.CheckAMA_CustomsOffice();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_CustomsOfficeInfo);
	}

	protected override void CheckAMA_VoyageMandatory()
	{
		var parent = Parent;
		if (parent.IsAir)
		{
			MandatoryValidation.WarnIfNotEntered(parent.AMA_VoyageInfo, parent.VoyageFlightNoLabel.Caption);
		}
		else
		{
			base.CheckAMA_VoyageMandatory();
		}
	}

	protected override void CheckAMA_MasterBill()
	{
		base.CheckAMA_MasterBill();
		var parent = Parent;
		var masterBill = parent.AMA_MasterBill;
		if (Parent.IsSea & !masterBill.IsLettersAndNumbersOnlyOrEmpty)
		{
			parent.AMA_MasterBillInfo.AddMessageError(Res.GetString("52EC51E9-CDB3-4076-A695-CA58A5083757", "You have entered invalid MBL/BOL."));
		}
	}

	protected override void CheckAMA_LloydsNumber()
	{
		base.CheckAMA_LloydsNumber();
		if (Parent.IsSea)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_LloydsNumberInfo);
		}
	}

	protected override void CheckAMA_MessageStatus()
	{
		base.CheckAMA_MessageStatus();
		ListValidation.ErrorIfInvalidCode(Parent.AMA_MessageStatusInfo);
	}
}
