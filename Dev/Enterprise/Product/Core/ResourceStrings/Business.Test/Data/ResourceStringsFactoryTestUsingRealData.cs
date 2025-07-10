using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class ResourceStringsFactoryTestUsingRealData : TestCase
	{
		public void TestReadonlySource()
		{
			var result = ResourceStringsFactory.Lookup(Res.DefaultLanguage, "46D4B528-A46D-4f8a-9A5E-97A8035E5765");
			AssertNotNull(result);
			AssertEquals(true, result.HD_IsReadOnly);
			AssertEquals(true, result.ReadOnly);
			AssertEquals("Checking for current software version", result.HD_Caption);
			AssertEquals(false, result.HD_IsCheckedOut);
			var clone = result.Clone();
			AssertEquals(true, clone.HD_IsReadOnly);
			AssertEquals(true, clone.ReadOnly);
			AssertEquals(false, result.HD_IsCheckedOut);
			AssertEquals("Checking for current software version", clone.HD_Caption);

			ZQuery query = new ZQuery(HelpDataStringSchema.HD_Language, Res.DefaultLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_Code, "46D4B528-A46D-4f8a-9A5E-97A8035E5765");
			result = ResourceStringsFactory.Load(query)[0];
			AssertEquals(true, result.HD_IsReadOnly);
			AssertEquals(true, result.ReadOnly);
			AssertEquals("Checking for current software version", result.HD_Caption);
			AssertEquals(false, result.HD_IsCheckedOut);
			clone = result.Clone();
			AssertEquals(true, clone.HD_IsReadOnly);
			AssertEquals(true, clone.ReadOnly);
			AssertEquals(false, result.HD_IsCheckedOut);
			AssertEquals("Checking for current software version", clone.HD_Caption);

			// checked-in
			result = ResourceStringsFactory.Lookup(Core.SharedConstants.Languages.ChineseSimplified, "StmALog", false);
			AssertNotNull(result);
			AssertEquals(true, result.HD_IsReadOnly);
			AssertEquals(false, result.HD_IsCheckedOut);
			AssertEquals("事件", result.HD_Caption);
		}
	}
}
