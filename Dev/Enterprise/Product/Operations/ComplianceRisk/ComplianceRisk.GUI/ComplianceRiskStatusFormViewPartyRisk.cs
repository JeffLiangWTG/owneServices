using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.GUI
{
	public static class ComplianceRiskStatusFormViewPartyRisk
	{
		public static void AddModuleActionsMenuItem(IDeniedPartyScreeningActionsProvider provider)
		{
			provider.AddParentModuleActionsMenuItem(new ZMenuItem(ResString.GetMultilingualString("FF2A23ED-8C79-4916-9515-53E570C870E3", "View Party Risk"), async delegate
			{
				if (provider.ModuleHasSelectedBusinessObjectsWithShowMessage())
				{
					var selectJobs = provider.ParentModuleFilterGrid.GetSelectedBusinessObjects().Select(u => ComplianceRiskStatusFormSynchronizer.TryGetValidComplianceRiskProviderBizO(u)).Where(x => x is not null).ToArray();
					if (selectJobs.Length > 0)
					{
						var factory = selectJobs[0].Factory;
						var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, selectJobs.Select(x => x.ParentID));
						var complianceRiskStatuses = factory.Load<ComplianceRiskStatus>(query).ToDictionary(x => x.COR_ParentID);

						var partyScreens = selectJobs.Select(u =>
						{
							var needSync = false;
							if (!complianceRiskStatuses.TryGetValue(u.ParentID, out var complianceRiskStatus))
							{
								complianceRiskStatus = null;
								needSync = true;
							}

							return new PartyScreenStruct(new ComplianceRiskBusinessObject((IBusiness)u, complianceRiskStatus), u, needSync);
						}).ToArray();

						foreach (var partyScreen in partyScreens)
						{
							if (partyScreen.NeedSync)
							{
								ComplianceRiskStatusSynchronizer.Synchronize(partyScreen.ComplianceBizO);
							}
						}

						ZExceptionReporting.ProcessWithSaveExceptionHandling(() => factory.Save(), null);
						await PartiesScreening(((ZFilterGridModule)provider.ParentModuleFilterGrid).LocateMainForm(), partyScreens);
					}
				}
			}));
		}

		static async Task PartiesScreening(Form form, PartyScreenStruct[] partyScreens)
		{
			var sourceBusinessObjectWithPartiesList = new List<IDpsSourceWithParties>();
			var screeningParties = new List<ScreeningParty>();

			foreach (var partyScreen in partyScreens)
			{
				var sourceBusinessObjectWithParties = new DpsSourceWithParties((BusinessObject)partyScreen.SourceBizO, partyScreen.ComplianceBizO.CompliancePartyRiskStatusProvider.Parties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).ToArray());

				sourceBusinessObjectWithPartiesList.Add(sourceBusinessObjectWithParties);
				screeningParties.AddRange(sourceBusinessObjectWithParties.ScreenParties);
			}

			var presentationManager = ObjectFactory.Get<IDeniedPartyScreeningPresentationManager>();
			if (screeningParties.Count > 0)
			{
				await presentationManager.PerformScreening(form, sourceBusinessObjectWithPartiesList, screeningParties.ToArray(), false, !DeniedPartyScreenerAsync.HasExcludedList(partyScreens[0].SourceBizO.Factory),
					complianceRiskAction: new ComplianceRiskAction(
						synchronizeAndSaveIfNeededAction: () =>
						{
							var factory = partyScreens[0].SourceBizO.Factory;
							foreach (var partyScreen in partyScreens)
							{
								ComplianceRiskStatusSynchronizer.Synchronize(partyScreen.ComplianceBizO, false);
							}

							ZExceptionReporting.ProcessWithSaveExceptionHandling(() => factory.Save(), null);
						},
						showMessageIfNeededAction: (List<ZString> jobComplianceStatus) =>
						{
							if (jobComplianceStatus.Count != partyScreens.Length)
							{
								return;
							}

							var messageBuilder = new System.Text.StringBuilder();
							for (int i = 0; i < partyScreens.Length; i++)
							{
								if (partyScreens[i].ComplianceBizO.ComplianceRiskStatus.COR_OverallRisk == jobComplianceStatus[i])
								{
									continue;
								}
								var jobRef = CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)partyScreens[i].SourceBizO);
								var statusCode = partyScreens[i].ComplianceBizO.ComplianceRiskStatus.COR_OverallRisk;
								var status = partyScreens[i].ComplianceBizO.ComplianceRiskStatus.Lookups.OverallRiskStatusCodes[statusCode].Description;
								messageBuilder.AppendLine(ResString.GetMultilingualString("019BD17B-0826-43C0-9E6E-DFBCE2A82963", "{0} will be set to {1}.", jobRef, status));
							}

							if (messageBuilder.Length > 0)
							{
								Globals.Message.Show(messageBuilder.ToString());
							}
						},
						updateVisibilityIfNeededAction: null,
						jobComplianceStatus: partyScreens.Select(u => u.ComplianceBizO.ComplianceRiskStatus.COR_OverallRisk).ToList()
					));
			}
		}

		struct PartyScreenStruct
		{
			public ComplianceRiskBusinessObject ComplianceBizO { get; }
			public IComplianceItemRiskStatusProvider SourceBizO { get; }
			public bool NeedSync { get; }

			public PartyScreenStruct(ComplianceRiskBusinessObject complianceBizO, IComplianceItemRiskStatusProvider sourceBizO, bool needSync)
			{
				ComplianceBizO = complianceBizO;
				SourceBizO = sourceBizO;
				NeedSync = needSync;
			}
		}
	}
}
