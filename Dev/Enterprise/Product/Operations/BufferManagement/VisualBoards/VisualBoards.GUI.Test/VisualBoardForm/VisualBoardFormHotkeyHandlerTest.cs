using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI.Test
{
	class VisualBoardFormHotkeyHandlerTest : VisualBoardsTestCase
	{
		public void TestHandleHotkeys()
		{
			var descriptor = (IBoardSectionDescriptor)new DummySectionDescriptor();

			using (ObjectFactory.Substitute("VisualBoardSectionDescriptors", new ArrayList { descriptor }))
			{
				var board = Factory.NewWithValidTestData<BMBoard>();
				var section = board.Sections.AddNew();

				section.MS_SectionType = descriptor.Type;

				Factory.Save();

				using (DisableAsyncBehaviour())
				using (var form = new VisualBoardFormWithExposedKeyHandling(VisualBoardsTestHelper.CreateSlideshowViewModel(board)))
				{
					form.Show();

					var control = form.FindSingle<HotkeyHandlerControl>();
					var msg = new Message();

					AssertSequencesEqual(Array.Empty<Keys>(), control.HandledKeys);

					form.ProcessCmdKey_Exposed(ref msg, Keys.A);
					form.ProcessCmdKey_Exposed(ref msg, Keys.S);

					AssertSequencesEqual(new[] { Keys.A, Keys.S }, control.HandledKeys);

					form.ProcessCmdKey_Exposed(ref msg, Keys.S);

					AssertSequencesEqual(new[] { Keys.A, Keys.S, Keys.S }, control.HandledKeys);
				}
			}
		}

		#region Implementation

		class VisualBoardFormWithExposedKeyHandling : VisualBoardForm
		{
			internal VisualBoardFormWithExposedKeyHandling(BoardSlideshowViewModel viewModel)
				: base(viewModel)
			{
			}

			internal void ProcessCmdKey_Exposed(ref Message msg, Keys keyData) => base.ProcessCmdKey(ref msg, keyData);
		}

		class DummySectionDescriptor : IBoardSectionDescriptor
		{
			string IBoardSectionDescriptor.Type => "HRC";

			string IBoardSectionDescriptor.Description => "Hillary Rodham Clinton";

			IEnumerable<TabSpec> IBoardSectionDescriptor.GetAdditionalTabs()
			{
				yield break;
			}

			IBoardSectionConfigurationBizo IBoardSectionDescriptor.GetSectionConfigurationBizo(IBMBoardSection section)
			{
				return new DummyBoardSectionConfigurationBizo();
			}

			object IBoardSectionDescriptor.GetSectionConfigurationControl()
			{
				throw new NotImplementedException();
			}

			IBoardSectionControl IBoardSectionDescriptor.GetSectionControl(IBMBoardSection section, BoardSectionViewModel sectionViewModel)
			{
				return new HotkeyHandlerControl(sectionViewModel);
			}

			BoardSectionViewModel IBoardSectionDescriptor.GetViewModel(IBMBoardSection section, BoardViewModel boardViewModel)
			{
				return new BoardSectionViewModel(section, boardViewModel);
			}
		}

		class HotkeyHandlerControl : Control, IBoardSectionControl, IHotkeyHandler
		{
			internal HotkeyHandlerControl(BoardSectionViewModel sectionViewModel)
			{
				this.sectionViewModel = sectionViewModel;
			}

			readonly BoardSectionViewModel sectionViewModel;

			public List<Keys> HandledKeys { get; } = new List<Keys>();

			bool IHotkeyHandler.ShouldHandle(Keys pressedKeys) => true;

			void IHotkeyHandler.HandleHotkeys(Keys pressedKeys)
			{
				HandledKeys.Add(pressedKeys);
			}

			string IBoardSectionControl.SectionType => "HRC";

			BoardSectionViewModel IBoardSectionControl.SectionViewModel => sectionViewModel;

			event EventHandler<BoardRefreshEventArgs> IBoardSectionControl.RefreshCompleted
			{
				add { }
				remove { }
			}

			bool IBoardSectionControl.AcceptDraggedControl(object control) => false;

			void IBoardSectionControl.Refresh(BoardRefreshEventArgs args)
			{
			}

			bool IBoardSectionControl.SuppressBoardRefresh(BoardRefreshEventArgs args) => false;
		}

		#endregion
	}
}
