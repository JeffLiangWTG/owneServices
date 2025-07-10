using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ImportJobDeclarationValidationSpecificTest : TestCaseWithFactory
	{
		public void TestIsVoyageFlightMandatory()
		{
			FakeJobDeclaration fakeDeclaration = FakeJobDeclaration.New(Factory);
			AssertEquals(typeof(FakeJobDeclarationValidation), fakeDeclaration.Validation.GetType());
			fakeDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			fakeDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			fakeDeclaration.IsVoyageUsedInMessages = false;

			fakeDeclaration.IsVoyageMandatory = true;
			fakeDeclaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrors(fakeDeclaration.JE_VoyageFlightNoInfo);
			fakeDeclaration.IsVoyageMandatory = false;
			fakeDeclaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrors(fakeDeclaration.JE_VoyageFlightNoInfo);
		}

		public void TestIsVoyageFlightNoUsedInMessages()
		{
			FakeJobDeclaration fakeDeclaration = FakeJobDeclaration.New(Factory);
			AssertEquals(typeof(FakeJobDeclarationValidation), fakeDeclaration.Validation.GetType());
			fakeDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			fakeDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			fakeDeclaration.IsVoyageMandatory = false;

			fakeDeclaration.IsVoyageUsedInMessages = true;
			fakeDeclaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrors(fakeDeclaration.JE_VoyageFlightNoInfo);
			fakeDeclaration.IsVoyageUsedInMessages = false;
			fakeDeclaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoNotifications(fakeDeclaration.JE_VoyageFlightNoInfo);
		}

		#region Fake Classes

		class FakeJobDeclarationValidation : ImportJobDeclarationValidation
		{
			public FakeJobDeclarationValidation(FakeJobDeclaration jobDeclaration, bool voyageIsMandatory, bool voyageIsUsedInMessages)
				: base(jobDeclaration)
			{
				fIsVoyageFlightNoMandatory = voyageIsMandatory;
				fIsVoyageFlightNoUsedInMessages = voyageIsUsedInMessages;
			}

			protected override bool IsVoyageFlightNoMandatory
			{
				get { return fIsVoyageFlightNoMandatory; }
			}

			protected override bool IsVoyageFlightNoUsedInMessages
			{
				get { return fIsVoyageFlightNoUsedInMessages; }
			}

			readonly bool fIsVoyageFlightNoMandatory;
			readonly bool fIsVoyageFlightNoUsedInMessages;
		}

		class FakeJobDeclaration : JobDeclaration
		{
			public FakeJobDeclaration(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public static new FakeJobDeclaration New(BusinessObjectFactory factory)
			{
				return factory.New<FakeJobDeclaration>();
			}

			protected override Customs.Business.JobDeclarationValidation GetNewValidation()
			{
				return new FakeJobDeclarationValidation(this, IsVoyageMandatory, IsVoyageUsedInMessages);
			}

			public bool IsVoyageMandatory = true;
			public bool IsVoyageUsedInMessages = true;
		}

		#endregion

	}
}
