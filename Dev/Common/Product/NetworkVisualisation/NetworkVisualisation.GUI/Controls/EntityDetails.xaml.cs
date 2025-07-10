using System.Windows;
using CargoWise.Main.Navigation.WPF;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	public partial class EntityDetails
	{
		#region Dependency Properties

		public static readonly DependencyProperty HeadingFontSizeProperty = DependencyProperty.Register("HeadingFontSize", typeof(double), typeof(EntityDetails), new UIPropertyMetadata(defaultValue: 20.0));
		public static readonly DependencyProperty HeadingFontWeightProperty = DependencyProperty.Register("HeadingFontWeight", typeof(NodeFontWeight), typeof(EntityDetails), new UIPropertyMetadata(defaultValue: NodeFontWeight.Bold));
		public static readonly DependencyProperty JobDetailsFontSizeProperty = DependencyProperty.Register("JobDetailsFontSize", typeof(double), typeof(EntityDetails), new UIPropertyMetadata(defaultValue: 12.0));
		public static readonly DependencyProperty CompletionCriteriaFontSizeProperty = DependencyProperty.Register("CompletionCriteriaFontSize", typeof(double), typeof(EntityDetails), new UIPropertyMetadata(defaultValue: 12.0));
		public static readonly DependencyProperty NotesFontSizeProperty = DependencyProperty.Register("NotesFontSize", typeof(double), typeof(EntityDetails), new UIPropertyMetadata(defaultValue: 12.0));
		public static readonly DependencyProperty ShowJobDetailsProperty = DependencyProperty.Register("ShowJobDetails", typeof(bool), typeof(EntityDetails), new UIPropertyMetadata(defaultValue: true));
		public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(NodeTextAlignment), typeof(EntityDetails), new UIPropertyMetadata(defaultValue: NodeTextAlignment.Left));
		public static readonly DependencyProperty IsNonScheduledProperty = DependencyProperty.Register("IsNonScheduled", typeof(bool), typeof(EntityDetails));

		#endregion

		public EntityDetails()
		{
			InitializeComponent();

			Loaded += OnLoaded;
		}

		void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			XamlTranslator.GetControlLoadedEvent<EntityDetails>().Invoke(sender, routedEventArgs);
			HeadingTextBox.Visibility = IsNonScheduled ? Visibility.Hidden : Visibility.Visible;
			NonScheduledHeadingTextBox.Visibility = IsNonScheduled ? Visibility.Visible : Visibility.Hidden;
		}

		#region Properties

		public double HeadingFontSize
		{
			get { return (double)GetValue(HeadingFontSizeProperty); }
			set { SetValue(HeadingFontSizeProperty, value); }
		}

		public NodeFontWeight HeadingFontWeight
		{
			get { return (NodeFontWeight)GetValue(HeadingFontWeightProperty); }
			set { SetValue(HeadingFontWeightProperty, value); }
		}

		public double JobDetailsFontSize
		{
			get { return (double)GetValue(JobDetailsFontSizeProperty); }
			set { SetValue(JobDetailsFontSizeProperty, value); }
		}

		public double CompletionCriteriaFontSize
		{
			get { return (double)GetValue(CompletionCriteriaFontSizeProperty); }
			set { SetValue(CompletionCriteriaFontSizeProperty, value); }
		}

		public double NotesFontSize
		{
			get { return (double)GetValue(NotesFontSizeProperty); }
			set { SetValue(NotesFontSizeProperty, value); }
		}

		public bool ShowJobDetails
		{
			get { return (bool)GetValue(ShowJobDetailsProperty); }
			set { SetValue(ShowJobDetailsProperty, value); }
		}

		public NodeTextAlignment TextAlignment
		{
			get { return (NodeTextAlignment)GetValue(TextAlignmentProperty); }
			set { SetValue(TextAlignmentProperty, value); }
		}

		public bool IsNonScheduled
		{
			get { return (bool)GetValue(IsNonScheduledProperty); }
			set { SetValue(IsNonScheduledProperty, value); }
		}

		public string NonScheduledHeadingText => Res.GetString("8562be54-e30d-4ac2-9d1d-e77e96203c40", "Non-Scheduled Items");

		#endregion
	}
}
