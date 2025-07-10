using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AdditionalContingencyData))]
	public class AdditionalContingencyDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOriginPremiseList()
		{
			var addtionalData = new AdditionalContingencyData(Factory);
			AssertEquals(typeof(CMREstablishmentCodesCollection), addtionalData.OriginPremiseList.GetType());
		}

		public void TestOriginPremiseValidation()
		{
			AdditionalContingencyData additionalData = new AdditionalContingencyData(Factory);
			additionalData.OriginPremise = ZString.Empty;
			AssertHasErrors("Origin", additionalData.OriginPremiseInfo);
			additionalData.OriginPremise = "12345";
			AssertNoErrors("Origin", additionalData.OriginPremiseInfo);
		}

		public void TestOriginPremiseReadOnly()
		{
			var addtionalData = new AdditionalContingencyData(Factory);
			Assert("Flag is false", !addtionalData.IsOriginPremiseReadOnly);
			Assert("Is not read only", !addtionalData.OriginPremiseInfo.ReadOnly);
			addtionalData.IsOriginPremiseReadOnly = true;
			Assert("Flag is true", addtionalData.IsOriginPremiseReadOnly);
			Assert("Is read only", addtionalData.OriginPremiseInfo.ReadOnly);
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestOriginPremisMaxLength()
		{
			AdditionalContingencyData additionalData = new AdditionalContingencyData(Factory);
			try
			{
				additionalData.OriginPremise = "123456";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdditionalContingencyData(Factory);
		}
	}
}
