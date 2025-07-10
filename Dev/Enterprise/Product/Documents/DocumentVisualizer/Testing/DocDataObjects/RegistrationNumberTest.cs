using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;
using UniversalRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(RegistrationNumber))]
	sealed class RegistrationNumberTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreate()
		{
			var universalRegistrationNumber = new UniversalRegistrationNumber
			{
				CountryOfIssue = new Enterprise.UniversalDataBuss.DataObjects.Universal.Country
				{
					Code = "AU",
					Name = "Australia"
				},
				Type = new UniversalDataBuss.DataObjects.Universal.RegistrationNumberType
				{
					Code = "ABN",
					Description = "Australian Business Number"
				},
				Value = "41 065 894 724",
			};

			var registrationNumber = RegistrationNumber.Create(Context, universalRegistrationNumber);

			AssertEquals(nameof(registrationNumber.CountryOfIssue.Code), "AU", registrationNumber.CountryOfIssue.Code);
			AssertEquals(nameof(registrationNumber.CountryOfIssue.Name), "Australia", registrationNumber.CountryOfIssue.Name);

			AssertEquals(nameof(registrationNumber.Type.Code), "ABN", registrationNumber.Type.Code);
			AssertEquals(nameof(registrationNumber.Type.Description), "Australian Business Number", registrationNumber.Type.Description);

			AssertEquals(nameof(registrationNumber.Value), "41 065 894 724", registrationNumber.Value);
		}

		public void TestCreate_Null()
		{
			var registrationNumber = RegistrationNumber.Create(Context, null);

			AssertEquals(nameof(registrationNumber.CountryOfIssue.Code), ZString.Empty, registrationNumber.CountryOfIssue.Code);
			AssertEquals(nameof(registrationNumber.CountryOfIssue.Name), ZString.Empty, registrationNumber.CountryOfIssue.Name);

			AssertEquals(nameof(registrationNumber.Type.Code), ZString.Empty, registrationNumber.Type.Code);
			AssertEquals(nameof(registrationNumber.Type.Description), ZString.Empty, registrationNumber.Type.Description);

			AssertEquals(nameof(registrationNumber.Value), ZString.Empty, registrationNumber.Value);
		}

		IContext Context => context ?? (context = ObjectFactory.Get<IContext>("IContext", Factory));
		IContext context;
	}
}
