using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ParameterizedStringRegistryItemEditorInfo : RegistryEditorInfo
	{
		public ParameterizedStringRegistryItemEditorInfo(params MultilingualString[] parameterDescriptions)
		{
			this.parameterDescriptions = parameterDescriptions;
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(ResourceString); }
		}

		public IList<MultilingualString> ParameterDescriptions
		{
			get { return parameterDescriptions; }
		}

		readonly MultilingualString[] parameterDescriptions;
	}
}
