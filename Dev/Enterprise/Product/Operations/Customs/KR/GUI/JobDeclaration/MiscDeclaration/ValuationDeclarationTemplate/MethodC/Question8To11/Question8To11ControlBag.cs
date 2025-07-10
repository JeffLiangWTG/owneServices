using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Question8To11ControlBag : ControlBag
	{
		public static Question8To11ControlBag Instance => instance ?? (instance = new Question8To11ControlBag());

		[ThreadStatic]
		static Question8To11ControlBag instance;

		public Question8To11ControlBag()
		{
			Question8Label = RegisterControl(nameof(Question8To11UserControl.Question8Label));
			Question8ALabel = RegisterControl(nameof(Question8To11UserControl.Question8ALabel));
			Question8ADropEdit = RegisterControl(nameof(Question8To11UserControl.Question8ADropEdit));
			Question8BLabel = RegisterControl(nameof(Question8To11UserControl.Question8BLabel));
			Question8BDropEdit = RegisterControl(nameof(Question8To11UserControl.Question8BDropEdit));
			Question8CLabel = RegisterControl(nameof(Question8To11UserControl.Question8CLabel));
			Question8CDropEdit = RegisterControl(nameof(Question8To11UserControl.Question8CDropEdit));
			Question8DLabel = RegisterControl(nameof(Question8To11UserControl.Question8DLabel));
			Question8DDropEdit = RegisterControl(nameof(Question8To11UserControl.Question8DDropEdit));

			Question9Label = RegisterControl(nameof(Question8To11UserControl.Question9Label));
			Question9ALabel = RegisterControl(nameof(Question8To11UserControl.Question9ALabel));
			Question9ADropEdit = RegisterControl(nameof(Question8To11UserControl.Question9ADropEdit));
			Question9BLabel = RegisterControl(nameof(Question8To11UserControl.Question9BLabel));
			Question9BDropEdit = RegisterControl(nameof(Question8To11UserControl.Question9BDropEdit));

			Question10Label = RegisterControl(nameof(Question8To11UserControl.Question10Label));
			Question10ALabel = RegisterControl(nameof(Question8To11UserControl.Question10ALabel));
			Question10ADropEdit = RegisterControl(nameof(Question8To11UserControl.Question10ADropEdit));
			Question10BLabel = RegisterControl(nameof(Question8To11UserControl.Question10BLabel));
			Question10BDropEdit = RegisterControl(nameof(Question8To11UserControl.Question10BDropEdit));
			Question10CLabel = RegisterControl(nameof(Question8To11UserControl.Question10CLabel));
			Question10CDropEdit = RegisterControl(nameof(Question8To11UserControl.Question10CDropEdit));
			Question10DLabel = RegisterControl(nameof(Question8To11UserControl.Question10DLabel));
			Question10DDropEdit = RegisterControl(nameof(Question8To11UserControl.Question10DDropEdit));

			Question11Label = RegisterControl(nameof(Question8To11UserControl.Question11Label));
			Question11ALabel = RegisterControl(nameof(Question8To11UserControl.Question11ALabel));
			Question11ADropEdit = RegisterControl(nameof(Question8To11UserControl.Question11ADropEdit));
			Question11BLabel = RegisterControl(nameof(Question8To11UserControl.Question11BLabel));
			Question11BDropEdit = RegisterControl(nameof(Question8To11UserControl.Question11BDropEdit));
			Question11CLabel = RegisterControl(nameof(Question8To11UserControl.Question11CLabel));
			Question11CDropEdit = RegisterControl(nameof(Question8To11UserControl.Question11CDropEdit));
			Question11DLabel = RegisterControl(nameof(Question8To11UserControl.Question11DLabel));
			Question11DDropEdit = RegisterControl(nameof(Question8To11UserControl.Question11DDropEdit));
		}

		protected override Control CreateTemplate() => new Question8To11UserControl();
		
		public ControlReference Question8Label { get; }
		public ControlReference Question8ALabel { get; }
		public ControlReference Question8ADropEdit { get; }
		public ControlReference Question8BLabel { get; }
		public ControlReference Question8BDropEdit { get; }
		public ControlReference Question8CLabel { get; }
		public ControlReference Question8CDropEdit { get; }
		public ControlReference Question8DLabel { get; }
		public ControlReference Question8DDropEdit { get; }

		public ControlReference Question9Label { get; }
		public ControlReference Question9ALabel { get; }
		public ControlReference Question9ADropEdit { get; }
		public ControlReference Question9BLabel { get; }
		public ControlReference Question9BDropEdit { get; }

		public ControlReference Question10Label { get; }
		public ControlReference Question10ALabel { get; }
		public ControlReference Question10ADropEdit { get; }
		public ControlReference Question10BLabel { get; }
		public ControlReference Question10BDropEdit { get; }
		public ControlReference Question10CLabel { get; }
		public ControlReference Question10CDropEdit { get; }
		public ControlReference Question10DLabel { get; }
		public ControlReference Question10DDropEdit { get; }

		public ControlReference Question11Label { get; }
		public ControlReference Question11ALabel { get; }
		public ControlReference Question11ADropEdit { get; }
		public ControlReference Question11BLabel { get; }
		public ControlReference Question11BDropEdit { get; }
		public ControlReference Question11CLabel { get; }
		public ControlReference Question11CDropEdit { get; }
		public ControlReference Question11DLabel { get; }
		public ControlReference Question11DDropEdit { get; }
	}
}
