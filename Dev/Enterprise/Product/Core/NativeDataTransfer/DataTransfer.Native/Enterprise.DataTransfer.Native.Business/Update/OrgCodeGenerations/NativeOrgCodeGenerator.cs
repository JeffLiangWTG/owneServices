using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations
{
	public class NativeOrgCodeGenerator : IOrgCodeGenerator
	{
		#region IOrgCodeGenerator Members

		public string Generate(CargoWise.Organizations.CodeGeneration.IOrgCodeInfo info, BusinessObjectFactory factory)
		{
			var generator = new OrgCodeGenerator();
			var algorithm = generator.GetAlgorithm(info);
			if (algorithm.NeedsUNLOCO() && string.IsNullOrWhiteSpace(info.UnlocoCode))
			{
				throw new NativeXMLUserVisibleException("The UNLOCO provided in the [ClosestPort.Code] element must not be empty when Organization Code Generation is enabled");
			}
			if (algorithm.NeedsName() && string.IsNullOrWhiteSpace(info.OH_FullName))
			{
				throw new NativeXMLUserVisibleException("Full Name could not be empty when Organization Code Generation is enabled");
			}
			if (algorithm.NeedsCountry() && (string.IsNullOrWhiteSpace(info.UnlocoCode) || string.IsNullOrWhiteSpace(info.CountryCode)))
			{
				throw new NativeXMLUserVisibleException("Due to Organization Code Generation settings, either the UNLOCO provided in the [ClosestPort.Code] element must not be empty, or a Country must be provided.");
			}

			return generator.GenerateCode(info, factory).GetProposedCode();
		}

		#endregion
	}
}
