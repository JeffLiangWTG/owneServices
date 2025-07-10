using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class PanelLayout
	{
		readonly List<IControlBag> controlBags = new List<IControlBag>();
		readonly List<PanelLayoutColumn> columns = new List<PanelLayoutColumn>();
		readonly HashSet<ControlReference> includedControls = new HashSet<ControlReference>();
		readonly Dictionary<ControlReference, IControlCaption> captionSettings = new Dictionary<ControlReference, IControlCaption>();
		readonly Dictionary<ControlReference, ControlBehaviourContainerCollection> controlBehaviours = new Dictionary<ControlReference, ControlBehaviourContainerCollection>();

		public PanelLayout()
		{
			AddColumn();
		}

		public IReadOnlyList<PanelLayoutColumn> Columns => columns;

		public bool CollapseEmptyRows { get; set; }

		public PanelLayoutTabSequence TabSequence { get; set; } = PanelLayoutTabSequence.ColumnWise;

		public bool ShouldInclude(ControlReference controlReference)
		{
			return includedControls.Contains(controlReference);
		}

		public IReadOnlyList<IControlBag> ControlBags => controlBags;
		public IReadOnlyCollection<ControlReference> IncludedControls => includedControls;

		/// <summary>
		/// Returns controls in their tab order regardless of their visibility.
		/// </summary>
		public IEnumerable<ControlReference> GetControlsInTabOrder() => columns.SelectMany(c => c.Rows).SelectMany(r => r.Parts).OfType<ControlReference>();

		public void RegisterControlBag(IControlBag controlBag)
		{
			controlBags.Add(controlBag);
		}

		/// <summary>
		/// Creates a ruler to align next control horizontally at the given position.
		/// </summary>
		/// <param name="position">The unscaled position of the ruler.</param>
		public PanelLayoutRuler CreateRuler(int position)
		{
			return new PanelLayoutRuler(position);
		}

		/// <summary>
		/// Creates a ruler that will adjust width of controls on its left side to align their right borders at the given position.
		/// </summary>
		/// <param name="position">The unscaled position of the ruler.</param>
		public PanelLayoutRightRuler CreateRightRuler(int position)
		{
			return new PanelLayoutRightRuler(position);
		}

		public int AddColumn()
		{
			int columnIndex = columns.Count;
			columns.Add(new PanelLayoutColumn());
			return columnIndex;
		}

		public PanelLayoutRow Include(params IPanelLayoutPart[] parts)
		{
			return Include(0, parts);
		}

		public PanelLayoutRow Include(int columnIndex, params IPanelLayoutPart[] parts)
		{
			var column = columns[columnIndex];
			var row = new PanelLayoutRow();
			column.Add(row);
			foreach (var controlLayoutInfo in parts)
			{
				Include(row, controlLayoutInfo);
			}
			return row;
		}

		void Include(PanelLayoutRow row, IPanelLayoutPart part)
		{
			if (part is ControlReference control)
			{
				if (includedControls.Contains(control))
				{
					throw new InvalidOperationException(FormattableString.Invariant($"Control '{control}' is already included into layout '{this}'."));
				}

				CheckBelongsToThisLayout(control);
				includedControls.Add(control);
			}

			row.Add(part);
		}

		public void SetVisibility<T>(ControlReference controlReference, Func<T, bool> isVisible, params Func<T, ZPropertyInfo>[] dependencies) where T : BusinessObject
		{
			CheckBelongsToThisLayout(controlReference);
			if (!controlBehaviours.TryGetValue(controlReference, out var controlBehaviourContainerCollection))
			{
				controlBehaviourContainerCollection = new ControlBehaviourContainerCollection();
				controlBehaviours[controlReference] = controlBehaviourContainerCollection;
			}

			controlBehaviourContainerCollection.Add(new ControlBehaviourContainer<T>(new ControlVisibilityBehaviour<T>(isVisible), dependencies));
		}

		public void SetCaption<T>(ControlReference controlReference, Func<T, ResourceStringData> getCaptionData, params Func<T, ZPropertyInfo>[] dependencies) where T : BusinessObject
		{
			SetCaptions(controlReference, (d) => new Dictionary<string, ResourceStringData> { { controlReference.Name, getCaptionData(d) } }, dependencies);
		}

		public void SetCaptions<T>(ControlReference controlReference, Func<T, IDictionary<string, ResourceStringData>> getCaptionData, params Func<T, ZPropertyInfo>[] dependencies) where T : BusinessObject
		{
			CheckBelongsToThisLayout(controlReference);
			captionSettings[controlReference] = new ControlCaption<T>(getCaptionData, dependencies);
		}

		public bool IsVisible(ControlReference controlReference, BusinessObject dataItem)
		{
			if (TryGetVisibilityBehaviourContainer(controlReference, out var visibilityBehaviourContainer) && visibilityBehaviourContainer?.ControlBehaviour is IControlVisibilityBehaviour controlVisibilityBehaviour)
			{
				return controlVisibilityBehaviour.IsVisible(dataItem);
			}

			return true;
		}

		public bool TryGetCaption(ControlReference controlReference, BusinessObject dataItem, out ResourceStringData caption)
		{
			caption = default;
			var result = false;
			if (TryGetCaptionData(controlReference, dataItem, out var captionData))
			{
				result = captionData.TryGetValue(controlReference.ControlName, out caption) && caption != null;
			}
			return result;
		}

		public bool TryGetCaptionData(ControlReference controlReference, BusinessObject dataItem, out IDictionary<string, ResourceStringData> captionData)
		{
			if (captionSettings.TryGetValue(controlReference, out var complexControlCaption))
			{
				return complexControlCaption.TryGetCaption(dataItem, out captionData);
			}

			captionData = new Dictionary<string, ResourceStringData>();
			return false;
		}

		public IEnumerable<ZPropertyInfo> GetCaptionDependencies(ControlReference controlReference, BusinessObject dataItem)
		{
			if (captionSettings.TryGetValue(controlReference, out var controlCaption))
			{
				return controlCaption.GetDependencies(dataItem);
			}

			return Enumerable.Empty<ZPropertyInfo>();
		}

		void CheckBelongsToThisLayout(ControlReference control)
		{
#if DEBUG
			if (!ControlBags.Contains(control.Bag))
			{
				throw new InvalidOperationException(FormattableString.Invariant($"'{this}' layout does not have a bag associated with control '{control}'."));
			}

			if (!control.Bag.ContainsControl(control.Name))
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Control '{control.Name}' is not part of {control.Bag}."));
			}
#endif
		}

		public ControlBehaviourContainerCollection GetControlBehaviours(ControlReference controlReference)
		{
			if (controlBehaviours.TryGetValue(controlReference, out var stateContainers))
			{
				return stateContainers;
			}

			return new ControlBehaviourContainerCollection();
		}

		public void SetBehaviours(ControlReference controlReference, ControlBehaviourContainerCollection controlBehaviourContainers)
		{
			CheckBelongsToThisLayout(controlReference);
			controlBehaviours[controlReference] = controlBehaviourContainers;
		}

		public IEnumerable<ZPropertyInfo> GetVisibilityDependencies(ControlReference controlReference, BusinessObject dataItem)
		{
			if (TryGetVisibilityBehaviourContainer(controlReference, out var visibilityBehaviourContainer))
			{
				return visibilityBehaviourContainer.GetDependencies(dataItem);
			}

			return Enumerable.Empty<ZPropertyInfo>();
		}

		public IControlBehaviourContainer GetControlBehaviourContainer<TBehaviourType>(ControlReference controlReference)
			where TBehaviourType : ControlBehaviour
		{
			return GetControlBehaviourContainerByBehaviourName(controlReference, typeof(TBehaviourType).FullName);
		}

		public IControlBehaviourContainer GetControlBehaviourContainerByBehaviourName(ControlReference controlReference, string controlBehaviourName)
		{
			var currentControlBehaviours = GetControlBehaviours(controlReference);
			if (currentControlBehaviours.TryGetValue(controlBehaviourName, out var behaviourContainer))
			{
				return behaviourContainer;
			}

			return null;
		}

		public bool HasBehaviour<TBehaviourType>(ControlReference controlReference) => HasBehaviourByBehaviourType(controlReference, typeof(TBehaviourType));

		public bool HasBehaviourByBehaviourType(ControlReference controlReference, Type behaviourType)
		{
			var behaviourName = behaviourType.IsGenericType ? behaviourType.GetGenericTypeDefinition().FullName : behaviourType.FullName;
			return HasBehaviourByBehaviourName(controlReference, behaviourName);
		}

		public bool HasBehaviourByBehaviourName(ControlReference controlReference, string behaviourName)
		{
			if (controlBehaviours.TryGetValue(controlReference, out var behaviourContainerCollection))
			{
				return behaviourContainerCollection.ContainsKey(behaviourName);
			}

			return false;
		}

		public TBehaviour GetControlBehaviour<TBehaviour>(ControlReference controlReference) where TBehaviour : ControlBehaviour
		{
			return GetControlBehaviourContainer<TBehaviour>(controlReference)?.ControlBehaviour as TBehaviour;
		}

		public ControlBehaviour GetControlBehaviourByBehaviourName(ControlReference controlReference, string controlBehaviourName)
		{
			return GetControlBehaviourContainerByBehaviourName(controlReference, controlBehaviourName)?.ControlBehaviour;
		}

		internal bool HasVisibilityBehaviour(ControlReference controlReference)
		{
			return TryGetVisibilityBehaviourContainer(controlReference, out _);
		}

		bool TryGetVisibilityBehaviourContainer(ControlReference controlReference, out IControlBehaviourContainer visibilityBehaviourContainer)
		{
			var controlVisibilityBehaviourKey = typeof(ControlVisibilityBehaviour<>).GetGenericTypeDefinition().FullName;
			visibilityBehaviourContainer = GetControlBehaviourContainerByBehaviourName(controlReference, controlVisibilityBehaviourKey);
			return visibilityBehaviourContainer != null;
		}
	}

	public enum PanelLayoutTabSequence
	{
		ColumnWise = 0,
		RowWise = 1
	}
}
