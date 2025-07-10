using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business;

public class NctsFallbackConfigurationValidation(NctsFallbackConfiguration parent) : ZValidation(parent)
{
	public override void ValidateAll()
	{
		ValidateStart();
		ValidateCustomsIncidentNumber();
	}

	public void ValidateStart()
	{
		ValidateCalculatedProperty(parent.StartInfo);
	}

	protected void CheckStart()
	{
		MandatoryValidation.CheckEntered(parent.StartInfo);
	}

	public void ValidateCustomsIncidentNumber()
	{
		ValidateCalculatedProperty(parent.CustomsIncidentNumberInfo);
	}

	protected void CheckCustomsIncidentNumber()
	{
		MandatoryValidation.CheckEntered(parent.CustomsIncidentNumberInfo);
	}

	public override Type AutoValidationType => typeof(NctsFallbackConfiguration);
}
