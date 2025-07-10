using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(Parameter))]
	sealed class ParameterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateCode()
		{
			var eventReference = new EventReference(Events.CustomisableEvent00Code, ZString.Empty);

			var param1 = new Parameter(Events.CustomisableEvent00Code, null, "AUMEL");
			param1.ValidateCode();
			AssertHasError(param1.CodeInfo, "Please enter a value.");

			param1.Code = "LOC";
			param1.ValidateCode();
			AssertNoErrors(param1.CodeInfo);

			var param2 = new Parameter(Events.CustomisableEvent00Code, "LOC", "AUMEL");

			eventReference.ParameterCollection.Add(param1);
			eventReference.ParameterCollection.Add(param2);
			param2.ValidateCode();
			AssertHasError(param2.CodeInfo, "The parameter LOC has been duplicated and must be unique.");
		}

		public void TestValidateParamValue()
		{
			var param1 = new Parameter(Events.CustomisableEvent00Code, "LOC", ZString.Empty);
			param1.ValidateParamValue();
			AssertHasError(param1.ParamValueInfo, "Please enter a value.");

			param1.ParamValue = "AUSYD";
			param1.ValidateParamValue();
			AssertNoErrors(param1.ParamValueInfo);
		}

		public void TestCodeList()
		{
			var lookupList = new Parameter(Events.CustomisableEvent00Code).CodeList;
			var expected = new CodeDescriptionPairList();

			foreach (FieldInfo codeField in typeof(CargoWise.EventReference.Constants.EventReferenceParameters.Codes).GetFields())
			{
				var code = codeField.GetValue(null).ToString().ToUpperInvariant();
				var description = typeof(Constants.EventReferenceParameters.Descriptions).GetProperties().FirstOrDefault(x => x.Name == codeField.Name);

				if (description != null && description.GetValue(null) != null)
				{
					var descriptionValue = description.GetValue(null) as MultilingualString;

					if (descriptionValue != null)
					{
						expected.AddPair(code, descriptionValue);
					}
					else
					{
						expected.AddPair(code, description.ToString());
					}
				}
				else
				{
					expected.AddPair(code, string.Empty);
				}
			}
			expected.Sort();

			AssertContainsExactElementsInExactOrder(expected, lookupList);
		}
	}
}
