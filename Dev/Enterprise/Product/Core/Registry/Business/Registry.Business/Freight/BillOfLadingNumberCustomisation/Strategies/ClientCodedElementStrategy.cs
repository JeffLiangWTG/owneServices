using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	internal class ClientCodedElementStrategy : ElementStrategy
	{
		public ClientCodedElementStrategy(string key)
			: base()
		{
			this.key = key;
		}
		readonly string key;

		public override string Key
		{
			get { return key; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard-coded constant")]
		public override string Name
		{
			get
			{
				switch (key)
				{
					case BillOfLadingNumberCustomisationElement.Keys.ClientCoded1:
						return "Custom Element 1";
					case BillOfLadingNumberCustomisationElement.Keys.ClientCoded2:
						return "Custom Element 2";
					case BillOfLadingNumberCustomisationElement.Keys.ClientCoded3:
						return "Custom Element 3";
					default:
						throw new NotSupportedException("Key is not a Client Coded Key: " + key);
				}
			}
		}

		public override bool Force
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return false; }
		}
		public override bool UseDetail
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return true; }
		}
		public override int DetailMaxLength
		{
			get { return 1024; }
		}

		public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
		{
			return parent.Detail.IsMacro() ? 0 : parent.Detail.Length;
		}

		public override BillOfLadingNumberCustomisationElementValidation GetValidation(BillOfLadingNumberCustomisationElement parent)
		{
			return new BillOfLadingNumberCustomisationElementClientCodedValidation(parent);
		}

		/*
		 * Output example: (.*) or "AAA"
		 * Check the text is marcro result or specific value
		 * 
		 * When it is macro, the output is (.*)
		 * That it would be anything, we accept any value since we cannot know the macro result.
		 * 
		 * When it is not macro, the output is what user typed on Registry Element.
		 * If user typed "AAA" on the registry, we expect the "AAA" is the result.
		 * So does "BBB", "CCC" or any value that is not macro.
		 */
		public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent)
		{
			return parent.Detail.IsMacro()
				? (NoResString)"(.*)"
				: parent.Detail;
		}
	}
}
