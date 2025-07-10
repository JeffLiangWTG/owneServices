using System;
using CargoWise.EntityFramework.Testing;
namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public abstract class CommonPreviousDocumentConfigurationTestCase<T> : TestCaseWithFactory
	where T : CommonPreviousDocumentConfiguration, new()
{
	protected T Configuration { get; private set; }

	public void TestDerivedClassIsSealed()
	{
		if (typeof(T) == typeof(CommonPreviousDocumentConfiguration))
		{
			Assert(true);
		}
		else
		{
			Assert("Derived class should be sealed", typeof(T).IsSealed);
		}
	}

	public void TestGetValidationDecider()
	{
		CombineAssertions(() =>
		{
			AssertType("NCTS5 Departure", ExpectedCommonPreviousDocumentDepartureValidationDeciderType, Configuration.GetValidationDecider(GetDepartureCommonPreviousDocument()));
			AssertType("NCTS5 Arrival", ExpectedCommonPreviousDocumentArrivalValidationDeciderType, Configuration.GetValidationDecider(GetArrivalCommonPreviousDocument()));
		});
	}

	protected abstract Type ExpectedCommonPreviousDocumentDepartureValidationDeciderType { get; }

	protected abstract Type ExpectedCommonPreviousDocumentArrivalValidationDeciderType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		Configuration = new();
	}

	CommonPreviousDocument GetDepartureCommonPreviousDocument()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.PreviousDocuments.AddNew();
	}

	CommonPreviousDocument GetArrivalCommonPreviousDocument()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.PreviousDocuments.AddNew();
	}
}
