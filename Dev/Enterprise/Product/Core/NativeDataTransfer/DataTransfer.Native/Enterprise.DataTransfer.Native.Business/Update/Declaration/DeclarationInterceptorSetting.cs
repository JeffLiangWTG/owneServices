using System;
using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using static Enterprise.DataTransfer.Native.Business.XmlConstants;

namespace Enterprise.DataTransfer.Native.Business.Update.Declaration
{
	internal class DeclarationInterceptorSetting : BaseInterceptorSetting
	{
		internal DeclarationInterceptorSetting(HeaderData source)
		{
			Source = source;
		}

		public HeaderData Source;

		public override IEnumerable<string> EnableList => new[] { EntitySetNames.Declaration };

		public override IEnumerable<string> DisableList => Array.Empty<string>();
	}
}
