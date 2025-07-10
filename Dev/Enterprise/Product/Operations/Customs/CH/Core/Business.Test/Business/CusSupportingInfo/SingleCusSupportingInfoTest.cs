using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class SingleCusSupportingInfoTest<T> : Customs.Business.Testing.CusSupportingInfoTest<T>
														where T : CusSupportingInfo
{
	protected abstract T GetNewCusSupportingInfo(BusinessObjectFactory factory);

	protected abstract IEnumerable<string> GetUsedFieldsNames();

	public virtual void TestOnSaving() => CombineAssertions(() =>
	{
		var supportingInfo = GetNewCusSupportingInfo(Factory);

		Factory.Save();
		AssertEquals("Not Saved in DB", false, supportingInfo.IsInDatabase);

		foreach (var propertyName in GetUsedFieldsNames())
		{
			supportingInfo = supportingInfo.IsDeleted ? GetNewCusSupportingInfo(Factory) : supportingInfo;

			var propertyInfo = supportingInfo.ZPropertyInfoHash[propertyName];
			propertyInfo.SetValueFromString("Y");
			Factory.Save();
			AssertEquals($"Saved in DB when {propertyName} is not empty", true, supportingInfo.IsInDatabase);

			propertyInfo.Value = propertyInfo.DefaultValue;
			Factory.Save();
			AssertEquals($"Deleted from DB when {propertyName} is empty", true, supportingInfo.IsDeleted);
		}
	});

	public virtual void TestIsSavedByFactory()
	{
		var supportingInfo = GetNewCusSupportingInfo(Factory);
		Assert("All fields are empty.", !supportingInfo.IsSavedByFactory);

		foreach (var propertyName in GetUsedFieldsNames())
		{
			var propertyInfo = supportingInfo.ZPropertyInfoHash[propertyName];
			propertyInfo.SetValueFromString(propertyInfo is ZPropertyInfoBool ? "Y" : "0");
			Assert($"{propertyName} is not empty.", supportingInfo.IsSavedByFactory);
			propertyInfo.Value = propertyInfo.DefaultValue;
		}
	}
}
