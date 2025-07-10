using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.GUI
{
	static class SupplementaryHelperUI
	{
		public static List<IZForm> NewRelatedDeclaration(JobDeclaration declaration)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
			var forms = new List<IZForm>();
			CreateSupplementaryDeclaration(declaration, x => x.RelatedDeclaration(),
				x => x.ForEach(y => forms.Add(controller.ShowFormForNewEntity(y))));
			return forms;
		}

		public static void NewEntryInstruction(JobDeclaration declaration)
		{
			CreateSupplementaryDeclaration(declaration, x => x.NewInstruction(),
				x => Globals.Message.Show(Res.GetString("36c87b0c-46df-4bee-9719-44be47623d37", "Entry Instruction created")));
		}

		public static void ReUseEntryInstruction(JobDeclaration declaration)
		{
			CreateSupplementaryDeclaration(declaration, x => x.ReuseInstruction(),
				x => Globals.Message.Show(Res.GetString("a9bc2c41-9888-48e9-bcac-54a6dbb759bb", "Entry Instruction updated")));
		}

		static void CreateSupplementaryDeclaration(JobDeclaration declaration, Func<DeclarationCreator, List<BaseJobDeclaration>> createMethod, Action<List<BaseJobDeclaration>> afterMethod)
		{
			var filter = new SelectClearedSimplifiedEntryHeaders(declaration);
			if (filter.EntryHeaders.Any())
			{
				var creator = DeclarationCreator.GetDeclarationCreator(declaration, filter);
				var result = createMethod(creator);
				afterMethod(result);
			}
			else
			{
				Globals.Message.Show(Res.GetString("04d2a8f0-2431-44db-b75b-507faa95e8d9", "No suitable simplified entry could be found on this declaration. This may mean that there are no entries that are deemed a simplified entry, or none that are customs cleared, or none that have an entry number or MRN."));
			}
		}
	}
}
