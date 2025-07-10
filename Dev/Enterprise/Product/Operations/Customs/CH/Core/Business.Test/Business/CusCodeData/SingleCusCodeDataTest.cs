
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class SingleCusCodeDataTest<T> : Customs.Business.Testing.CusCodeDataTest<T>
														where T : CusCodeData
{
	protected abstract T GetNewCusCodeData(BusinessObjectFactory factory);

	protected abstract IEnumerable<string> GetUsedFieldsNames();

	public void TestOnSaving() => CombineAssertions(() =>
	{
		var cusCodeData = GetNewCusCodeData(Factory);

		Factory.Save();
		AssertEquals("Not Saved in DB", false, cusCodeData.IsInDatabase);

		foreach (var propertyName in GetUsedFieldsNames())
		{
			cusCodeData = cusCodeData.IsDeleted ? GetNewCusCodeData(Factory) : cusCodeData;

			var propertyInfo = cusCodeData.ZPropertyInfoHash[propertyName];
			propertyInfo.SetValueFromString(propertyInfo is ZPropertyInfoBool ? "Y" : "0");
			Factory.Save();
			AssertEquals($"Saved in DB when {propertyName} is not empty", true, cusCodeData.IsInDatabase);

			propertyInfo.Value = propertyInfo.DefaultValue;
			Factory.Save();
			AssertEquals($"Deleted from DB when {propertyName} is empty", true, cusCodeData.IsDeleted);
		}
	});

	public void TestIsSavedByFactory()
	{
		var cusCodeData = GetNewCusCodeData(Factory);
		Assert("All fields are empty.", !cusCodeData.IsSavedByFactory);

		foreach (var propertyName in GetUsedFieldsNames())
		{
			var propertyInfo = cusCodeData.ZPropertyInfoHash[propertyName];
			propertyInfo.SetValueFromString(propertyInfo is ZPropertyInfoBool ? "Y" : "0");
			Assert($"{propertyName} is not empty.", cusCodeData.IsSavedByFactory);
			propertyInfo.Value = propertyInfo.DefaultValue;
		}
	}
}
