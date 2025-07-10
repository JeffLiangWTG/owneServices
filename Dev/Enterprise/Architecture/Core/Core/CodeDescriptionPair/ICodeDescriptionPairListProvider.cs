using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core
{
	public interface ICodeDescriptionPairListProvider
	{
		CodeDescriptionPairList CodeDescriptionPairList { get; }
	}

	public class CodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public CodeDescriptionPairListProvider(Func<CodeDescriptionPairList> getListFunc)
		{
			Argument.NotNull(getListFunc, nameof(getListFunc));
			this.getListFunc = getListFunc;
		}

		readonly Func<CodeDescriptionPairList> getListFunc;

		public CodeDescriptionPairList CodeDescriptionPairList
		{
			get
			{
				return fCodeDescriptionPairList ?? (fCodeDescriptionPairList = getListFunc());
			}
		}
		CodeDescriptionPairList fCodeDescriptionPairList;
	}
}
