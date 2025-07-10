using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.RegistryItemEditors.System.ProcessController.LoggingMethodsRegistryItemEditor, Enterprise.Registry.GUI")]
	class LoggingMethodsDataType : CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType
	{
		public LoggingMethodsDataType()
			: base(
				new LoggingMethods(),
				false,
				(EnvProxy.IsHostedWithCargowise || (EnvProxy.IsInternalSystem ?? false))
					? new[] { LoggingMethods.CFL, LoggingMethods.FSL }
					: new[] { LoggingMethods.FSL })
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, SystemDefinableCodeDescriptionBoolWithExtraBoolCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var values = proposedValue
				.Cast<SystemDefinableCodeDescriptionBoolWithExtraBool>()
				.ToList();

			if (!values
				.Any(systemDefinableCodeDescriptionBoolWithExtraBool => systemDefinableCodeDescriptionBoolWithExtraBool.Bool2))
			{
				throw new RegistryValidationException(Res.GetString("{3595D041-9DAC-42AA-B79F-757458F1DB52}", "You must select at least one logging method to 'Write to'"));
			}

			var readFrom = values
				.SingleOrDefault(b => b.Bool)
				?.Code;

			if (string.Equals(readFrom, LoggingMethods.SYS, StringComparison.OrdinalIgnoreCase))
			{
				throw new RegistryValidationException(Res.GetString("{BFE34A52-6371-440F-94D1-1F48FFC69D3D}", "Syslog can't be selected as the 'Read from' logging method"));
			}

			if (string.Equals(readFrom, LoggingMethods.CFL, StringComparison.OrdinalIgnoreCase))
			{
				throw new RegistryValidationException(Res.GetString("{8DE497CC-A397-4B8A-AC39-98BAC9909440}", "Combined File System can't be selected as the 'Read from' logging method"));
			}
		}
	}
}
