using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class DocumentInfoBuilder
	{
		public DocumentInfoBuilder(BusinessObject bizObj, IStmMenuItem menuItem)
		{
			this.bizObj = Argument.NotNull(bizObj, nameof(bizObj));
			this.menuItem = Argument.NotNull(menuItem, nameof(menuItem));
		}

		readonly BusinessObject bizObj;
		readonly IStmMenuItem menuItem;

		public IReadOnlyCollection<IDocumentInfo> CreateDocumentInfos()
		{
			if (menuItem.SU_MenuType != Enterprise.Core.Constants.StmMenuItemTypes.Forms
				|| !bizObj.IsApplicable(menuItem.SU_FilterList))
			{
				return Array.Empty<IDocumentInfo>();
			}

			var matchingPivots = menuItem
				.Documents
				.OfType<IStmMenuTemplatePivot>()
				.Where(pivot => bizObj.IsApplicable(pivot.SI_MenuTemplateFilter))
				.ToArray();

			if (matchingPivots.Length == 0)
			{
				return Array.Empty<IDocumentInfo>();
			}

			var orderedPivots = matchingPivots
				.OrderBy(p => p.PK == menuItem.SU_PrimaryDocPackItemId ? -1 : p.SI_Index)
				.ToArray();

			IServiceContainer container = new ServiceContainer();
			container.Register<IEventBroker>(new EventBroker());

			var pack = new DocumentInfoPack(container, bizObj, null, menuItem.SU_MenuNameMultilingual, orderedPivots);

			return pack
				.DocumentInfos
				.ToArray();
		}
	}
}
