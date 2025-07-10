using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	class DocumentFilterBool : DocumentFilterBase
	{
		public DocumentFilterBool(string filterName, Func<IeDoc, ZBool> boolGetter)
		{
			Argument.NotNull(boolGetter, nameof(boolGetter));

			FilterName = filterName;
			BoolGetter = boolGetter;
		}

		string FilterName { get; }

		Func<IeDoc, ZBool> BoolGetter { get; }

		internal ZBool? Value { get; private set; }

		public override void AddFilterValue(ZString value)
		{
			var newValue = Parse(value);
			if (Value.HasValue && newValue != Value.Value)
			{
				throw new DataObjectReadFailureException($"Both True and False are specified for filter {FilterName}.");
			}
			Value = newValue;
		}

		public override bool IsMatch(IeDoc eDoc)
		{
			return !Value.HasValue || Value == BoolGetter(eDoc);
		}

		ZBool Parse(ZString textValue)
		{
			bool boolValue;
			if (bool.TryParse(textValue, out boolValue))
			{
				return boolValue;
			}
			else
			{
				throw new DataObjectReadFailureException("Cannot parse filter " + FilterName + " value from text '" + textValue + "'. Supported values: True, False.");
			}
		}
	}
}
