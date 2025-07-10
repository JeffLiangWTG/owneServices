using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class StaffLoginNamesRelatedToAllTasks : ValueProviderWithLoadControlFactory
	{
		public override Regex Regex => regex;

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<StaffLoginNamesRelatedToAllTasks(IWorkflowProvider)>",
				ResString.GetMultilingualString("94864E2E-6728-4D79-8EB2-F9166B488EE5", @"Return all the staff login names relate to the current job."),
				new List<(string example, object expectedResult)>
				{
					("<StaffLoginNamesRelatedToAllTasks()>", "Hunter.Yang")
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var macroMatch = Regex.Match(macro).Groups[1].Value;
			var destBizO = ValueProviderHelper.GetBusinessObjectFromDataProvider(macroMatch, report);

			if (destBizO == null || destBizO is not IWorkflowProvider)
			{
				ReportMacroError(report, Res.GetString("3206EBC9-1273-4247-B12E-95A9AE530DA8", "Business Object is not applicable for looking up related staff login names."));
				return null;
			}

			var workflowProvider = (IWorkflowProvider)destBizO;

			var tasks = workflowProvider.WorkflowItems.Tasks;
			var allAssignedStaffCode = new HashSet<ZString>();
			var allRequiredCapabilities = new HashSet<ZGuid>();
			foreach (ProcessTask task in tasks)
			{
				if (!task.P9_GS_NKAssignedStaffMember.IsEmpty)
				{
					allAssignedStaffCode.Add(task.P9_GS_NKAssignedStaffMember);
				}

				if (!task.P9_G4_RequiredCapability.IsEmpty)
				{
					allRequiredCapabilities.Add(task.P9_G4_RequiredCapability);
				}
			}

			var staff = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, allAssignedStaffCode));
			var capabilities = Factory.Load<GlbResourceCapabilityPivot>(new ZQuery(GlbResourceCapabilityPivotSchema.G5_G4_Capability, allRequiredCapabilities));
			if (capabilities.Length > 0)
			{
				staff = staff.Union(Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, capabilities.Select(c => c.G5_GS_Resource)))).ToArray();
			}

			var allStaffLoginNames = staff.Select(s => s.GS_LoginName).OrderBy(o => o);
			return string.Join(", ", allStaffLoginNames);
		}

		static readonly Regex regex = new Regex(@"^<(?:[\s]*)StaffLoginNamesRelatedToAllTasks(?:[\s]*)\((?:[\s]*)(\s*\S*\s*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
