#if !WINZOR
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CargoWise.Main.Navigation
{
	/// <summary>
	/// Interaction logic for SnapshotsControl.xaml
	/// </summary>
	public partial class SnapshotsControl : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public SnapshotsControl()
		{
			InitializeComponent();
			AddSnapshotCommand = new RelayCommand(_ => AddSnapshotClick(null, null));
		}

		public ICommand SaveCommand
		{
			get { return (ICommand)GetValue(SaveCommandProperty); }
			set { SetValue(SaveCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SaveCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SaveCommandProperty =
			DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(SnapshotsControl), new PropertyMetadata(null));

		public bool IsEditing
		{
			get { return (bool)GetValue(IsEditingProperty); }
			set { SetValue(IsEditingProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsEditing.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsEditingProperty =
			DependencyProperty.Register("IsEditing", typeof(bool), typeof(SnapshotsControl), new PropertyMetadata(false));

		public ObservableCollection<Snapshot> SnapShotsData
		{
			get { return (ObservableCollection<Snapshot>)GetValue(SnapShotsDataProperty); }
			set { SetValue(SnapShotsDataProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SnapShotsData.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SnapShotsDataProperty =
			DependencyProperty.Register("SnapShotsData", typeof(ObservableCollection<Snapshot>), typeof(SnapshotsControl), new PropertyMetadata(null, new PropertyChangedCallback(OnSnapShotsDataChanged)));

		public Style HeaderStyle
		{
			get { return (Style)GetValue(HeaderStyleProperty); }
			set { SetValue(HeaderStyleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for HeaderStyle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HeaderStyleProperty =
			DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(SnapshotsControl), new PropertyMetadata(null));

		public ICommand AddSnapshotCommand
		{
			get { return (ICommand)GetValue(AddSnapshotCommandProperty); }
			set { SetValue(AddSnapshotCommandProperty, value); }
		}

		public static readonly DependencyProperty AddSnapshotCommandProperty =
			DependencyProperty.Register("AddSnapshotCommand", typeof(ICommand), typeof(SnapshotsControl), new PropertyMetadata(null));

		static void OnSnapShotsDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SnapshotsControl snapshotsControl)
			{
				if (e.OldValue != null)
				{
					((ObservableCollection<Snapshot>)e.OldValue).CollectionChanged -= CollectionChanged;
				}
				if (e.NewValue != null)
				{
					snapshotsControl.IsEmpty = ((ObservableCollection<Snapshot>)e.NewValue).Count == 0;
					((ObservableCollection<Snapshot>)e.NewValue).CollectionChanged += CollectionChanged;
					snapshotsControl.OnPropertyChanged(nameof(SnapshotsWithOptionalAdd));
				}
			}
			void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
			{
				snapshotsControl.IsEmpty = snapshotsControl.SnapShotsData.Count == 0;
				snapshotsControl.OnPropertyChanged(nameof(SnapshotsWithOptionalAdd));
			}
		}

		public bool IsEmpty
		{
			get { return (bool)GetValue(IsEmptyProperty); }
			private set { SetValue(IsEmptyProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsEmpty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsEmptyProperty =
			DependencyProperty.Register("IsEmpty", typeof(bool), typeof(SnapshotsControl), new PropertyMetadata(true));

		void SaveChangesClick(object sender, RoutedEventArgs e)
		{
			SaveCommand?.Execute(null);
			IsEditing = false;
		}

		void CancelClick(object sender, RoutedEventArgs e)
		{
			IsEditing = false;
			if (DataContext is SnapshotsViewModel viewModel)
			{
				viewModel.Refresh();
			}
		}

		internal SnapshotDialog _dialog;

		internal SnapshotDialog Dialog
		{
			get
			{
				if (_dialog == null)
				{
					if (DataContext is SnapshotsViewModel viewModel)
					{
						_dialog = new SnapshotDialog(viewModel, this);
					}
				}

				return _dialog;
			}
		}

		void AddSnapshotClick(object sender, RoutedEventArgs e)
		{
			Dialog?.Show();
		}

		void EditClick(object sender, RoutedEventArgs e)
		{
			IsEditing = true;
		}

		public class AddSnapshotPlaceholder { }

		public ObservableCollection<object> SnapshotsWithOptionalAdd =>
			SnapShotsData == null
				? new ObservableCollection<object>()
				: new ObservableCollection<object>(
					SnapShotsData.Count < SnapshotsViewModel.MaxSnapshots && SnapShotsData.Count > 0
						? SnapShotsData.Cast<object>().Append(new AddSnapshotPlaceholder())
						: SnapShotsData.Cast<object>()
				);
	}
}
#endif
