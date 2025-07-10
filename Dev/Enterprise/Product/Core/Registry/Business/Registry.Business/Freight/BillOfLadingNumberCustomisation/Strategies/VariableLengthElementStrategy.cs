using System;
using CargoWise.Types;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	internal class VariableLengthElementStrategy : ElementStrategy
	{
		public VariableLengthElementStrategy(string key, string name, int defaultDetail, NumberCustomisationElementCategories category) : base()
		{
			ElementKey = key;
			ElementName = name;
			DefaultMaxLength = defaultDetail;
			Category = category;
		}

		public override string Key => ElementKey;

		public override string Name => ElementName;

		public override bool UseDetail => true;

		public override string DefaultDetail => DefaultMaxLength.ToString();

		public override int DetailMaxLength => DefaultDetail.Length;

		public override string Description => Res.GetString("c064e005-0f81-4eef-9125-780ecdf0899e", "{0} - the maximum length is {1}, if you set the length less than the maximum length, the string on the left will be preserved.", Name, DefaultDetail);

		public override BillOfLadingNumberCustomisationElementValidation GetValidation(BillOfLadingNumberCustomisationElement parent)
		{
			return new BillOfLadingNumberCustomisationElementVariableLengthValidation(parent, DefaultMaxLength);
		}

		public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
		{
			return ZInt.ParseSafe(parent.Detail, DefaultMaxLength);
		}

		public override NumberCustomisationElementCategories Categories => Category;

		public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent) => throw new NotImplementedException();

		string ElementKey { get; }
		string ElementName { get; }
		int DefaultMaxLength { get; }
		NumberCustomisationElementCategories Category { get; }
	}
}
