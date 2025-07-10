using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common.Enumeration;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.AllocateEDocs
{
	sealed class FileBrowserUserControlTest : TestCase
	{
		public void TestPrefetchesWhenPossible()
		{
			var fileSystem = new MockFileSystem(
				@"D1\D11\D111\",
				@"D1\D11\D112\",
				@"D1\D12\D111\",
				@"D1\D12\D112\",
				@"D2\"
			);

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Description = string.Empty;

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				fileBrowser.SetDataBinding(dummy, "Z0_Description");

				var tree = (ZTreeView)fileBrowser.Controls.Find("directoryTreeView", true).Single();
				dummy.Z0_Description = @"D1\D11\D111";

				AssertContainsExactElementsInAnyOrder("Expected to have prefetched everything", Array.Empty<string>(), fileSystem.ThingsYouProbablyWantToFetchFirst);

				var singleFetches = fileSystem.FetchRequests.Where(f => f.Length == 1).Select(f => f.Single()).ToArray();
				AssertEquals("We should always attempt to batch our fetch requests, otherwise there is no benefit to fetching. Single Fetches: \r\n" + string.Join("\r\n", singleFetches), 0, singleFetches.Length);
			}
		}

		public void TestExpandingNodePopulatesChildren()
		{
			var fileSystem = new MockFileSystem(
				@"R1\R1D1\",
				@"R1\R1D2\",
				@"R1\R1D3\"
			);

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				var tree = (ZTreeView)fileBrowser.Controls.Find("directoryTreeView", true).Single();
				var r1 = tree.Nodes["R1"];
				AssertEquals("There should be a placeholder, so the expand icon is shown", 1, r1.Nodes.Count);

				r1.Expand();
				AssertEquals("The children should have been populated", 3, r1.Nodes.Count);
			}
		}

		public void TestAddFileMenuItem()
		{
			const string menuItemKey = "ThingToSearchFor";
			var fileSystem = new MockFileSystem(
				   @"R1\R1D1\",
				   @"R1\R1D2\",
				   @"R1\R1D3\"
			   );

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				fileBrowser.AddFileMenuItem(new ZMenuItem(menuItemKey) { Name = menuItemKey });

				var grid = (ZGrid)fileBrowser.Controls.Find("filesGrid", true)[0];
				AssertNotNull("The menu item should be added to the appropriate spot in the files grid", grid.ContextMenu.MenuItems[menuItemKey]);
			}
		}

		public void TestRootsAreAdded()
		{
			var fileSystem = new MockFileSystem(
				@"R1\R1F1",
				@"R2\R2F1",
				@"R3\R3F1"
			);

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				var tree = (ZTreeView)fileBrowser.Controls.Find("directoryTreeView", true).Single();
				AssertHasSubdirectories("Should contain all the root nodes", new[] { "R1", "R2", "R3" }, tree.Nodes);
			}
		}

		void AssertHasSubdirectories(string message, string[] subdirs, TreeNodeCollection nodes)
		{
			var dirNames = nodes.Cast<TreeNode>().Select(n => n.Name);
			AssertContainsExactElementsInAnyOrder(message, subdirs, dirNames);
		}

		[RequiresSTA]
		public void TestGridIsReboundWhenValueChanged()
		{
			var fileSystem = new MockFileSystem(
							@"R1\R1F1",
							@"R2\R2F1",
							@"R3\R3F1"
						);

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				fileBrowser.SetDataBinding(dummy, "Z0_Description");

				var grid = (ZGrid)fileBrowser.Controls.Find("filesGrid", true).Single();

				dummy.Z0_Description = "R1";
				AssertEquals("The files grid should contain the selected dirs files", "R1F1", grid.List.Cast<FileBusinessObject>().Single().FileNameWithExtension);

				dummy.Z0_Description = "R2";
				AssertEquals("The files grid should contain the selected dirs files", "R2F1", grid.List.Cast<FileBusinessObject>().Single().FileNameWithExtension);

				dummy.Z0_Description = "R3";
				AssertEquals("The files grid should contain the selected dirs files", "R3F1", grid.List.Cast<FileBusinessObject>().Single().FileNameWithExtension);
			}
		}

		public void TestTreeSelectsCurrentDirectory()
		{
			var fileSystem = new MockFileSystem(
							@"R1\R1D1\R1D11\R1D111\",
							@"R1\R1D2\R1D21\R1D211\",
							@"R2\R2D1\R2D11\R2D111\",
							@"R2\R2D2\R2D21\R2D211\"
						);

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				fileBrowser.SetDataBinding(dummy, "Z0_Description");

				var tree = (ZTreeView)fileBrowser.Controls.Find("directoryTreeView", true).Single();

				dummy.Z0_Description = @"R1\R1D1\R1D11\R1D111";
				AssertEquals("The tree should have selected the correct node", "R1D111", tree.SelectedNode.Name);
				Assert("The node should be expanded to", ZEnumerable.Iterate(tree.SelectedNode.Parent, n => n.Parent, null).All(n => n.IsExpanded));

				dummy.Z0_Description = @"R2\R2D1\R2D11\R2D111";
				AssertEquals("The tree should have selected the correct node", "R2D111", tree.SelectedNode.Name);
				Assert("The node should be expanded to", ZEnumerable.Iterate(tree.SelectedNode.Parent, n => n.Parent, null).All(n => n.IsExpanded));
			}
		}

		public void TestTreeSelectsCorrectPathWithTSClient()
		{
			var fileSystem = new MockFileSystem(
						@"C\R1D1\R1D11\R1D111\",
						@"D\R2D1\R2D11\R2D111\"
					);

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				fileBrowser.SetDataBinding(dummy, "Z0_Description");

				var tree = (ZTreeView)fileBrowser.Controls.Find("directoryTreeView", true).Single();

				dummy.Z0_Description = @"\\tsclient\C\R1D1\R1D11\R1D111";
				AssertEquals("The tree should have selected the correct node", "R1D111", tree.SelectedNode.Name);
				Assert("The node should be expanded to", ZEnumerable.Iterate(tree.SelectedNode.Parent, n => n.Parent, null).All(n => n.IsExpanded));

				dummy.Z0_Description = @"\\TSCLIENT\D\R2D1\R2D11\R2D111";
				AssertEquals("The tree should have selected the correct node, and casing should be ignored", "R2D111", tree.SelectedNode.Name);
				Assert("The node should be expanded to", ZEnumerable.Iterate(tree.SelectedNode.Parent, n => n.Parent, null).All(n => n.IsExpanded));
			}
		}

		public void TestDoesntThrowExceptionWhenUserTypesInvalidPath()
		{
			var fileSystem = new MockFileSystem(
							@"R1\R1D1\R1D11\R1D111\",
							@"R1\R1D2\R1D21\R1D211\",
							@"R2\R2D1\R2D11\R2D111\",
							@"R2\R2D2\R2D21\R2D211\"
						);

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				AssertNoExceptionThrown("We're safe to do nothing when the user types a bogus path", () =>
				{
					fileBrowser.SetDataBinding(dummy, "Z0_Description");

					dummy.Z0_Description = @"foo";
					dummy.Z0_Description = @"bar\baz";
					dummy.Z0_Description = @"biz\bam\boozled";
				});
			}
		}

		[RequiresSTA]
		public void TestSelectingFileFiresEvent()
		{
			var fileSystem = new MockFileSystem(
							@"R1\R1F1",
							@"R1\R1F2",
							@"R1\R1F3",
							@"R1\R1F4",
							@"R1\R1F5",
							@"R1\R1F6");

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				fileBrowser.SetDataBinding(dummy, "Z0_Description");
				dummy.Z0_Description = "R1";

				var selectedFiles = new List<string>();
				fileBrowser.FileSelected += (o, e) => selectedFiles.Add(fileBrowser.SelectedFile);

				var grid = (ZGrid)fileBrowser.Controls.Find("filesGrid", true).Single();
				grid.Sort = "FileName";

				grid.CurrentCell = new DataGridCell(0, 1);
				grid.CurrentCell = new DataGridCell(2, 1);
				grid.CurrentCell = new DataGridCell(4, 1);

				var expected = new[]
				{
						@"R1\R1F1",
						@"R1\R1F3",
						@"R1\R1F5",
				};

				AssertArrayEqualsByElements("We should have had three events fired, one for each row we selected", expected, selectedFiles.ToArray());
			}
		}

		public void TestSelectedFilesIsEmptyWhenNothingIsSelected()
		{
			var fileSystem = new MockFileSystem(
							@"R1\R1F1",
							@"R1\R1F2",
							@"R1\R1F3",
							@"R1\R1F4",
							@"R1\R1F5",
							@"R1\R1F6");

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				var grid = (ZGrid)fileBrowser.Controls.Find("filesGrid", true).Single();
				grid.Sort = "FileName";
				grid.CurrentCell = new DataGridCell(-1, -1);

				AssertEquals("PRE: There should be no selected items", string.Empty, fileBrowser.SelectedFile);

				AssertEquals("We haven't selected anything, so SelectedFiles should be empty", 0, fileBrowser.SelectedFiles.Count());
			}
		}

		public void TestTimeoutException()
			=> AssertFileSystemExceptionsAreHandled(new TimeoutException(), "There was a timeout when trying to access the file-system. Please check your network connection.");

		[RequiresSTA]
		public void TestDirectoryNotFoundException()
			=> AssertFileSystemExceptionsAreHandled(new DirectoryNotFoundException(), "The directory can no longer be found.");

		public void TestUnauthorizedAccessException()
			=> AssertFileSystemExceptionsAreHandled(new UnauthorizedAccessException(), "You do not have permission to access this file.");

		public void TestIOException()
			=> AssertFileSystemExceptionsAreHandled(new IOException(), "An error occurred while attempting to access the directory.");

#if !WINZOR
		[RequiresSTA]
		public void TestEmptyRoots_WithListDirectory()
		{
			var terminalService = new Mock<RemoteDesktopServices.TerminalService>();
			terminalService.Setup(x => x.IsRemoteAppSession).Returns(true);

			var remoteChannelMock = new Mock<Enterprise.Integration.RemoteDesktopServices.IRemoteChannel>();
			remoteChannelMock.Setup(mock => mock.RegisteredRemoteMessageTypes).Returns(new string[] { RemoteDesktopServices.EnterpriseChannelMessageTypes.ListDirectory });
			remoteChannelMock.Setup(x => x.SendMessage<RemoteDesktopServices.MessageElements.ListDirectoryRequest[], RemoteDesktopServices.MessageElements.ListDirectoryResult[]>(
				It.IsAny<string>(),
				It.IsAny<RemoteDesktopServices.MessageElements.ListDirectoryRequest[]>()))
				.Returns(Array.Empty<RemoteDesktopServices.MessageElements.ListDirectoryResult>());

			using (ObjectFactory.Substitute(terminalService.Object))
			using (ObjectFactory.Substitute(remoteChannelMock.Object))
			{
				AssertNoExceptionAndRdpUnableAccessLocalDrivesMessage();
			}
		}

		public void TestEmptyRoots_Hosted_WithoutListDirectory()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var terminalService = new Mock<RemoteDesktopServices.TerminalService>();
			terminalService.Setup(x => x.IsRemoteAppSession).Returns(true);

			var remoteChannelMock = new Mock<Enterprise.Integration.RemoteDesktopServices.IRemoteChannel>();
			remoteChannelMock.Setup(mock => mock.RegisteredRemoteMessageTypes).Returns(new string[] { RemoteDesktopServices.EnterpriseChannelMessageTypes.CheckDriveMapping });

			using (ObjectFactory.Substitute(terminalService.Object))
			using (ObjectFactory.Substitute(remoteChannelMock.Object))
			{
				AssertNoExceptionAndRdpUnableAccessLocalDrivesMessage();
			}
		}

		void AssertNoExceptionAndRdpUnableAccessLocalDrivesMessage()
		{
			AssertNoExceptionThrown(() =>
			{
				using (var form = new ZForm())
				using (var fileBrowser = new FileBrowserUserControl())
				{
					fileBrowser.Dock = DockStyle.Fill;
					form.Controls.Add(fileBrowser);
					form.Show();

					AssertEquals("The Remote Desktop connection is unable to access your local drives. Please ensure access is allowed to these drives.", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
				}
			});
		}

		[RequiresSTA]
		public void TestEmptyRoots_SelfHosted_WithoutListDirectory()
		{
			EnvProxy.SetHostedLocationForTest("");
			var terminalService = new Mock<RemoteDesktopServices.TerminalService>();
			terminalService.Setup(x => x.IsRemoteAppSession).Returns(true);

			var remoteChannelMock = new Mock<Enterprise.Integration.RemoteDesktopServices.IRemoteChannel>();
			remoteChannelMock.Setup(mock => mock.RegisteredRemoteMessageTypes).Returns(new string[] { RemoteDesktopServices.EnterpriseChannelMessageTypes.CheckDriveMapping });

			using (ObjectFactory.Substitute(terminalService.Object))
			using (ObjectFactory.Substitute(remoteChannelMock.Object))
			{
				AssertNoExceptionThrown(() =>
				{
					var mockFileSystem = new MockTestFileSystem(new[] { "txt" });
					using (var form = new ZForm())
					using (var fileBrowser = new FileBrowserUserControl(mockFileSystem))
					{
						fileBrowser.Dock = DockStyle.Fill;
						form.Controls.Add(fileBrowser);
						form.Show();

						AssertNull(((UnitTestUserNotification)Globals.Message).LastMessage.Text);

						var tree = (ZTreeView)fileBrowser.Controls.Find("directoryTreeView", true).Single();
						AssertEquals("Should contain 1 the root node", 1, tree.Nodes.Count);
						AssertHasSubdirectories("root nodes should be C:", new[] { "C:" }, tree.Nodes);
					}
				});
			}
		}
#endif

		void AssertFileSystemExceptionsAreHandled(Exception ex, string expectedMessage)
		{
			AssertFileSystemExceptionHandled_Root(ex, expectedMessage);
			AssertFileSystemExceptionHandled_Expand(ex, expectedMessage);
			AssertFileSystemExceptionHandled_SelectPath(ex, expectedMessage);
		}

		void AssertFileSystemExceptionHandled_Root(Exception ex, string expectedMessage)
		{
			var fileSystem = new Mock<IFileSystem>();
			fileSystem.Setup(m => m.Roots).Throws(ex);
			fileSystem.Setup(m => m.GetDirectories(It.IsAny<string>())).Throws(ex);
			fileSystem.Setup(m => m.GetFiles(It.IsAny<string>())).Throws(ex);

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem.Object))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				AssertEquals("When an file system exception is thrown, we should handle it by showing a message to the user", expectedMessage, ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
				fileSystem.Verify(m => m.Roots);
			}
		}

		void AssertFileSystemExceptionHandled_SelectPath(Exception ex, string expectedMessage)
		{
			var fileSystem = new Mock<IFileSystem>();
			fileSystem.Setup(m => m.Roots).Returns(new[] { new DirectoryBusinessObject(fileSystem.Object, "R1") });
			fileSystem.Setup(m => m.GetDirectories("R1\\")).Returns(Enumerable.Empty<string>());
			fileSystem.Setup(m => m.GetFiles("R1\\")).Throws(ex);

			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem.Object))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				fileBrowser.SetDataBinding(dummy, "Z0_Description");
				dummy.Z0_Description = "R1\\";

				AssertEquals("When an file system exception is thrown, we should handle it by showing a message to the user", expectedMessage, ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
				fileSystem.Verify(m => m.GetFiles("R1\\"));
			}
		}

		void AssertFileSystemExceptionHandled_Expand(Exception ex, string expectedMessage)
		{
			var fileSystem = new Mock<IFileSystem>();

			fileSystem.Setup(m => m.CanAccessDirectory(It.IsAny<string>())).Returns(true);

			fileSystem.Setup(m => m.Roots).Returns(new[] { new DirectoryBusinessObject(fileSystem.Object, "R1\\") });
			fileSystem.Setup(m => m.GetDirectories("R1\\")).Returns(new[] { "R1\\R11\\" });
			fileSystem.Setup(m => m.GetDirectories("R1\\R11\\")).Throws(ex);

			fileSystem.Setup(m => m.GetFiles("R1\\")).Returns(Enumerable.Empty<string>());

			using (var form = new ZForm())
			using (var fileBrowser = new FileBrowserUserControl(fileSystem.Object))
			{
				fileBrowser.Dock = DockStyle.Fill;
				form.Controls.Add(fileBrowser);

				form.Show();

				var tree = (ZTreeView)fileBrowser.Controls.Find("directoryTreeView", true).Single();
				var r1 = tree.Nodes["R1"];
				AssertEquals("There should be a placeholder, so the expand icon is shown", 1, r1.Nodes.Count);

				r1.Expand();

				AssertEquals("When an file system exception is thrown, we should handle it by showing a message to the user", expectedMessage, ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
				fileSystem.Verify(m => m.Roots);
				fileSystem.Verify(m => m.GetDirectories("R1\\"));
				fileSystem.Verify(m => m.GetDirectories("R1\\R11\\"));
			}
		}
	}
}
