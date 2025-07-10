using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.WorkflowTemplates
{
	public class WorkflowTemplateSetting : BaseInterceptorSetting
	{
		public override IEnumerable<string> EnableList
		{
			get
			{
				return new[] { "WorkflowTemplate" };
			}
		}

		public override IEnumerable<string> DisableList
		{
			get
			{
				return System.Array.Empty<string>();
			}
		}
	}
}
