using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
public class ImportJobDeclarationFormTest : JobDeclarationFormTest<JobDeclaration>
{
	public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

	protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
	{
		var declaration = base.GetPopulatedDeclarationForFormBashingCore();

		// Suppress what they did in C:\Dev\Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Job\JobHeader\JobHeader.SetHasChangesIfHasErrors()
		// If HasChanges is set to true, TestBashingForm will complain that: JE_MessageType = 'IMP', JE_MessageSubType = '', JE_TransportMode = 'SEA' BusinessEntity.HasChanges was set to true on tab page: BillingTabPage with Text: Billing on TabControl: MainTabControl  
		Factory.SetContext(BusinessContext.DisableSetHasChangesIfHasErrors_ForTestOnly);

		return declaration;
	}
}
