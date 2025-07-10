using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	class DocumentFilterCode : DocumentFilterBase
	{
		public DocumentFilterCode(Func<IeDoc, ZString> codeGetter)
		{
			Argument.NotNull(codeGetter, nameof(codeGetter));

			CodeGetter = codeGetter;
		}

		Func<IeDoc, ZString> CodeGetter { get; }

		internal List<ZString> Values { get; } = new List<ZString>();

		public override void AddFilterValue(ZString value)
		{
			var upperCaseValue = value.ToUpper();

			if (!Values.Contains(upperCaseValue))
			{
				Values.Add(upperCaseValue);
			}
		}

		public override bool IsMatch(IeDoc eDoc)
		{
			return Values.Count == 0 || Values.Contains(CodeGetter(eDoc).ToUpper());
		}
	}
}
