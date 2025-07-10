using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class C88EdocsSaver
	{
		public C88EdocsSaver(JobDeclaration dec)
		{
			this.declaration = dec;
		}

		public void RenderC88AndStoreInEdocs()
		{
			using (DisposableEnvironment.ForBranch(declaration.JE_GB.ToGuid()))
			{
				var docCommand = GetDocumentCommandThatWeWillFireAsIfUserClickedIt();
				if (docCommand != null)
				{
					SilentDocumentPrinter silentPrinter = new SilentDocumentPrinter(declaration.Factory, declaration, docCommand);
					silentPrinter.Print(ZGuid.Empty, 0, true);
				}
			}
		}

		DocumentCommand GetDocumentCommandThatWeWillFireAsIfUserClickedIt()
		{
			DocumentCommand result = null;
			DocumentSupporter documentSupporter = declaration.DocumentSupporter;
			if (documentSupporter != null)
			{
				var filter = new DocumentZQuery();
				var typeOfC88 = GBCustomsDataRegistry.Instance.ChiefC88EntryPrintPreference.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var pk = ZGuid.Empty;
				switch (typeOfC88)
				{
					case Core.Constants.ChiefC88Options.Code.Rich:
						pk = new ZGuid("D2131ED9-D39F-41DC-841D-9310595F3EDE");
						break;  // PK of the "SADH C88" menu option
					case Core.Constants.ChiefC88Options.Code.Plain:
						pk = new ZGuid("ED2D13CB-57FE-4F20-86F5-4ADC23E1C92D");
						break;      // "SADH C88 Plain Paper"
				}
				if (!pk.IsEmpty)
				{
					filter.AddToFilter(StmMenuItemSchema.PK, pk);
					result = declaration.Factory.LoadTop1<DocumentCommand>(filter);
				}
			}
			return result;
		}

		readonly JobDeclaration declaration;
	}
}
