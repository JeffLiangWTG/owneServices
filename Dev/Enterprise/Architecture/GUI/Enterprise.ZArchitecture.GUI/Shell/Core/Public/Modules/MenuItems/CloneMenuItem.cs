using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public class CloneMenuItem : ZMenuItem
	{
		public CloneMenuItem(ZGrid parentGrid, Func<IBusinessObjectCollection> collectionGetter, Shortcut shortcut = Shortcut.None)
			: base(ResString.GetMultilingualString("8C39703D-E135-4CAC-AB63-9536AC68BC46", "Clone"))
		{
			Click += CloneMenuItem_Click;
			Shortcut = shortcut;

			this.parentGrid = parentGrid;
			this.collectionGetter = collectionGetter;
			this.Name = "clone";
		}

		protected readonly ZGrid parentGrid;
		readonly Func<IBusinessObjectCollection> collectionGetter;

		void CloneMenuItem_Click(object sender, EventArgs e)
		{
			var clones = new Collection<CloneResult>();
			var collection = collectionGetter();

			BusinessObject[] selectedElements = parentGrid.SelectedElements.Length != 0
												? parentGrid.SelectedElements
												: new BusinessObject[1] { parentGrid.GetCurrent() };

			foreach (var element in selectedElements)
			{
				if (element != null)
				{
					var clone = Clone(element);
					if (clone != null)
					{
						collection.Add(clone);
						clones.Add(new CloneResult(element, clone));
					}
				}
			}

			OnElementsCloned(clones);
		}

		protected virtual BusinessObject Clone(BusinessObject source)
		{
			return source.Clone();
		}

		protected virtual void OnElementsCloned(Collection<CloneResult> cloneResults)
		{
		}

		protected class CloneResult
		{
			internal CloneResult(BusinessObject source, BusinessObject clone)
			{
				Source = source;
				Clone = clone;
			}

			public BusinessObject Source { get; private set; }
			public BusinessObject Clone { get; private set; }
		}
	}
}
