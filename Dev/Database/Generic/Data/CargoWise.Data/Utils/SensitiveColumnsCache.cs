using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	public class SensitiveColumnsCache
	{
		[ThreadSafe]
		public static SensitiveColumnsCache Instance { get; } = new SensitiveColumnsCache();

		public IEnumerable<string> SensitiveColumns
		{
			get
			{
				if (sensitiveColumns == null)
				{
					sensitiveColumns = new HashSet<string>()
					{
"CPR_ValueFrom",
"CZ_Password",
"GP_Certificate",
"GP_CertificateAuthority",
"GP_CertificatePassPhrase",
"GP_CertificateSerialNumber",
"GP_CurrentPassword",
"GP_NextPassword",
"GP_PasswordStatus",
"GS_PasswordHash",
"GS_PasswordHashIterations",
"GS_PasswordSalt",
"GS_SqlLoginPasswordHash",
"PW_Password",
"QH_AuthorisingOfficerID",
"S6_Password",
"@@VERSION",
"sp_cursorexecute",
"sp_execute",
"sp_executesql",
"sp_execute_external_script",
"EXEC",
"EXECUTE",
"SERVERPROPERTY",
					};
				}
				return sensitiveColumns;
			}
		}
		HashSet<string> sensitiveColumns;
	}
}
