using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public class CustomizeDocumentsUrlHandler : UrlHandler
	{
		internal CustomizeDocumentsUrlHandler()
			: this(new DocumentCustomisationDevTool())
		{
		}

		internal CustomizeDocumentsUrlHandler(IDevTool displayTool)
		{
			this.displayTool = displayTool;
		}

		protected override string[] GetQueryNamesSecuredBySecurityHash(QueryString queryString)
		{
			return new string[] { "LicenceCode", "TableCode", "BusinessEntityPK" };
		}

		protected override bool HandleCore(QueryString queryString)
		{
			var pk = GetBusinessEntityPk(queryString);
			var tableCode = queryString["TableCode"];
			if (string.IsNullOrEmpty(tableCode))
			{
				return false;
			}
			var bizObj = new ReadOnlyBusinessObjectFactory { NameForDebugging = "Customize Documents Url" }.Load(tableCode, pk);
			if (bizObj == null)
			{
				Globals.Message.ShowError(Res.GetString("19a86932-5b24-4927-9157-727cc935ea58", "Tried to open document customization without providing an entity."));
				return false;
			}

			if (!(bizObj is IDocumentSupportable))
			{
				Globals.Message.ShowError(Res.GetString("673be86d-0dac-4029-a85b-ea47a6a94584", "Tried to open document customization for an entity that does not support this functionality."));
				return false;
			}

#if !WINZOR
			ForceFormToActivate(LocateMainForm());
#endif
			using (var formToShow = new ZForm(bizObj))
			{
				displayTool.Show(formToShow);
			}
			return true;
		}

		protected override string ExpectedCommandText
		{
			get { return "CustomizeDocuments"; }
		}

		public static CustomizeDocumentsUrlHandler Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CustomizeDocumentsUrlHandler();
				}

				return instance;
			}
		}

		[ThreadStatic]
		static CustomizeDocumentsUrlHandler instance;
		readonly IDevTool displayTool;
	}
}
