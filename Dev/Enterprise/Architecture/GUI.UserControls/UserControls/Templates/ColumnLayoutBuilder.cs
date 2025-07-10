using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ColumnLayoutBuilder<T, BagT> : ICommonLayoutBuilder where T : BusinessObject where BagT : IControlBag
	{
		internal const int MediumWidth = 116; // 116 also matches width of ZDateEdit with date and time
		internal const int LongWidth = 293; // 293 is chosen to match width of ZAddressControl which is wide and does not support resizing
		internal const int LongControlWidth = 600; // 600 was set to align long control like SupportingInformationControl

		internal const int ColumnWidthOffset = 14;  // (CaptionWith=106) + (LongWidth= 293) + (Offset=14) = 413 was set to align second column with message status and custom status in Global Manifest 'Declaration Details' box

		readonly List<IControlBag> bags = new List<IControlBag>();
		readonly List<ControlInfo> controls = new List<ControlInfo>();
		readonly List<CaptionInfo> captions = new List<CaptionInfo>();
		readonly List<ControlBehaviourInfo> controlBehaviours = new List<ControlBehaviourInfo>();

		int currentColumn = -1;

		protected ColumnLayoutBuilder()
		{
			SetDefaultVisibilities();
			SetDefaultCaptions();
		}

		public abstract BagT CommonBag { get; }

		protected IEnumerable<IControlBag> Bags
		{
			get
			{
				if (!bags.Contains(CommonBag))
				{
					RegisterCommonBags();
				}
				return bags;
			}
		}

		public void AddControlBag(IControlBag bag)
		{
			bags.Add(bag);
		}

		void RegisterCommonBags()
		{
			if (CommonBag == null)
			{
				throw new InvalidOperationException("CommonBag cannot be null.");
			}
			AddControlBag(CommonBag);
		}

		public void AddColumn()
		{
#if DEBUG
			if (MaxColumns > 0 && currentColumn + 1 >= MaxColumns)
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Layout cannot have more than {MaxColumns} columns."));
			}
#endif
			currentColumn++;
		}

		public virtual ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

		protected internal virtual int MaxColumns => 2;

		protected internal virtual PanelLayoutTabSequence TabSequence => PanelLayoutTabSequence.ColumnWise;

		public virtual bool NarrowColumnForMediumControls => false;

		public void Add(ControlReference control, ControlWidthClass widthClass, ControlReference alignToControl = null, VerticalAlignment verticalAlignment = VerticalAlignment.Top)
		{
			if (currentColumn < 0)
			{
				throw new InvalidOperationException("You need to add a column before adding controls to it.");
			}

			controls.Add(new ControlInfo(currentColumn, control, widthClass, alignToControl, verticalAlignment));
		}

		public void SetVisibility(ControlReference controlReference, Func<T, bool> isVisible, params Func<T, ZPropertyInfo>[] dependencies)
		{
			AddControlBehaviour(controlReference, new ControlVisibilityBehaviour<T>(isVisible), dependencies);
		}

		public void SetCaption(ControlReference controlReference, Func<T, ResourceStringData> getCaptionData, params Func<T, ZPropertyInfo>[] dependencies)
		{
			SetCaptions(controlReference, (d) => new Dictionary<string, ResourceStringData>() { { controlReference.Name, getCaptionData(d) } }, dependencies);
		}

		public void SetCaptions(ControlReference controlReference, Func<T, IDictionary<string, ResourceStringData>> getCaptionsData, params Func<T, ZPropertyInfo>[] dependencies)
		{
			captions.Add(new CaptionInfo(controlReference, getCaptionsData, dependencies));
		}

		public void AddControlBehaviour(ControlReference controlReference, ControlBehaviour controlBehaviour, params Func<T, ZPropertyInfo>[] dependencies)
		{
			controlBehaviours.Add(new ControlBehaviourInfo(controlReference, controlBehaviour, dependencies));
		}

		public void AddControlBehaviour<TControl>(ControlReference controlReference, Action<TControl, T> updateControlBehaviourAction, params Func<T, ZPropertyInfo>[] dependencies)
			where TControl : Control
		{
			var customControlBehaviour = new InternalCustomizableControlBehaviour<TControl, T>(updateControlBehaviourAction);
			AddControlBehaviour(controlReference, customControlBehaviour, dependencies);
		}

		public void AddControlBehaviour<TControl>(ControlReference controlReference, string behaviourName, Action<TControl, T> updateControlBehaviourAction, params Func<T, ZPropertyInfo>[] dependencies)
			where TControl : Control
		{
			var customControlBehaviour = new InternalCustomizableControlBehaviour<TControl, T>(behaviourName, updateControlBehaviourAction);
			AddControlBehaviour(controlReference, customControlBehaviour, dependencies);
		}

		public PanelLayout Build()
		{
			var layout = new PanelLayout();
			layout.CollapseEmptyRows = true;

			var captionWidth = (int)CaptionWidth;
			var captionRuler = layout.CreateRuler(captionWidth);
			var mediumWidthRuler = layout.CreateRightRuler(captionWidth + MediumWidth);
			var longWidthRuler = layout.CreateRightRuler(captionWidth + LongWidth);
			var longControlRuler = layout.CreateRightRuler(LongControlWidth);

			foreach (var bag in Bags)
			{
				layout.RegisterControlBag(bag);
			}

			for (var i = 1; i <= currentColumn; i++)
			{
				layout.AddColumn();
			}

			foreach (var controlInfo in controls)
			{
				PanelLayoutRow row;
				var control = controlInfo.Control;
				var widthClass = controlInfo.WidthClass;
				var columnWidthRuler = NarrowColumnForMediumControls && widthClass == ControlWidthClass.Medium
						? layout.CreateRuler(captionWidth + MediumWidth + ColumnWidthOffset)
						: layout.CreateRuler(captionWidth + LongWidth + ColumnWidthOffset);

				switch (widthClass)
				{
					case ControlWidthClass.Auto:
						row = layout.Include(controlInfo.Column, captionRuler, control, columnWidthRuler);
						break;
					case ControlWidthClass.LongControl:
						row = layout.Include(controlInfo.Column, control, longControlRuler, columnWidthRuler);
						break;
					case ControlWidthClass.LongNoCaption:
						row = layout.Include(controlInfo.Column, control, longWidthRuler, columnWidthRuler);
						break;
					case ControlWidthClass.Medium:
						row = layout.Include(controlInfo.Column, captionRuler, control, mediumWidthRuler, columnWidthRuler);
						break;
					default:
						row = layout.Include(controlInfo.Column, captionRuler, control, longWidthRuler, columnWidthRuler);
						break;
				}

				row.AlignToControl = controlInfo.AlignToControl;
				row.VerticalAlignment = controlInfo.VerticalAlignment;
			}

			foreach (var captionInfo in captions)
			{
				layout.SetCaptions(captionInfo.ControlReference, captionInfo.GetCaptionsData, captionInfo.Dependencies);
			}

			var behaviourContainersByControl = GetBehaviourContainersByControl();

			foreach (var (controlReference, controlBehaviourContainerCollection) in behaviourContainersByControl)
			{
				layout.SetBehaviours(controlReference, controlBehaviourContainerCollection);
			}

			layout.TabSequence = TabSequence;

			return layout;
		}

		protected virtual void SetDefaultVisibilities() { }

		protected virtual void SetDefaultCaptions() { }

		IEnumerable<(ControlReference, ControlBehaviourContainerCollection)> GetBehaviourContainersByControl()
		{
			var behavioursByControl = controlBehaviours.GroupBy(c => c.ControlReference);

			foreach (var behaviour in behavioursByControl)
			{
				var containerCollection = new ControlBehaviourContainerCollection();

				foreach (var controlBehaviourContainer in behaviour.Select(s => new ControlBehaviourContainer<T>(s.ControlBehaviour, s.Dependencies)))
				{
					containerCollection.Add(controlBehaviourContainer);
				}

				yield return (behaviour.Key, containerCollection);
			}
		}

		class ControlInfo
		{
			public ControlInfo(int column, ControlReference control, ControlWidthClass withClass, ControlReference alignToControl = null, VerticalAlignment verticalAlignment = VerticalAlignment.Top)
			{
				Column = column;
				Control = control;
				AlignToControl = alignToControl;
				VerticalAlignment = verticalAlignment;
				WidthClass = withClass;
			}

			public int Column { get; }
			public ControlReference Control { get; }
			public ControlReference AlignToControl { get; }
			public VerticalAlignment VerticalAlignment { get; }
			public ControlWidthClass WidthClass { get; }
		}

		class CaptionInfo
		{
			public CaptionInfo(ControlReference controlReference, Func<T, IDictionary<string, ResourceStringData>> getCaptionsData, params Func<T, ZPropertyInfo>[] dependencies)
			{
				ControlReference = controlReference;
				GetCaptionsData = getCaptionsData;
				Dependencies = dependencies;
			}

			public ControlReference ControlReference { get; }
			public Func<T, IDictionary<string, ResourceStringData>> GetCaptionsData { get; }
			public Func<T, ZPropertyInfo>[] Dependencies { get; }
		}

		class ControlBehaviourInfo
		{
			public ControlBehaviourInfo(ControlReference controlReference, ControlBehaviour controlBehaviour, Func<T, ZPropertyInfo>[] dependencies)
			{
				ControlReference = controlReference;
				ControlBehaviour = controlBehaviour;
				Dependencies = dependencies;
			}

			public ControlReference ControlReference { get; }

			public ControlBehaviour ControlBehaviour { get; }

			public Func<T, ZPropertyInfo>[] Dependencies { get; }
		}
	}

	public enum ColumnLayoutBuilderCaptionWidthSize
	{
		Medium = 106, // 106 was set to align controls with country and registration number in Global Manifest 'Declaration Details' box
		Long = 150 // Discussion between SW and JPG to allow single column layouts to have extra length for the captions. Value based on Customs Declaration 'Organisations Tab'
	}
}
