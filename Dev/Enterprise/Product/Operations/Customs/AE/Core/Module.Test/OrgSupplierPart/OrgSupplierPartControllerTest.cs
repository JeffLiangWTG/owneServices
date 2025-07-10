using System;
using Enterprise.Customs.AE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(OrgSupplierPartController))]
public class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
{
	protected override string CountryCode
	{
		get
		{
			return Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates;
		}
	}

	protected override Type GetBusinessObjectType()
	{
		return typeof(OrgSupplierPart);
	}

	public override Type ControllerToBashType
	{
		get
		{
			return typeof(OrgSupplierPartController);
		}
	}
}
