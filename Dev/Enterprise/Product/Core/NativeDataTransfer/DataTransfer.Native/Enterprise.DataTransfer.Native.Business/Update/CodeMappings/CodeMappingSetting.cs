using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.CodeMappings
{
	public class CodeMappingSetting : BaseInterceptorSetting
	{
		public CodeMappingSetting(string ownerCode = "")
		{
			OwnerCode = ownerCode;
			Enable = true;
		}

		#region Properties

		public string OwnerCode { get; set; }

		#endregion

		#region Filter

		public override IEnumerable<string> EnableList
		{
			get { return System.Array.Empty<string>(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> DisableList
		{
			get
			{
				return new[] { "Organization" };
			}
		}

		#endregion
	}
}
