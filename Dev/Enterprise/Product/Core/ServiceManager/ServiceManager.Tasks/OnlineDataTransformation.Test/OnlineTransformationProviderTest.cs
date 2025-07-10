using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Testing
{
	sealed class OnlineTransformationProviderTest : TransactionedTestCase
	{
		public void TestWhenNoCurrentStateAllTaskAreSavedToState()
		{
			var newTask1 = new Transformation01();
			var newTask2 = new Transformation02();
			var newTask3 = new Transformation03();

			var allTasks = new IOnlineTransformation[] { newTask1, newTask2, newTask3 };

			EnsureNoState();

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				provider.GetRunningTasks();
			}
			AssertStateSavedCorrectly("{{\"Completed\":[],\"Pending\":[\"{0}\",\"{1}\",\"{2}\"]}}", allTasks);
		}

		public void TestNewTransformationsAreSavedToState()
		{
			var completedTask1 = new Transformation01();
			var completedTesk2 = new Transformation02();
			var pendingTask = new Transformation03();
			var newTask = new Transformation04();

			var allTasks = new IOnlineTransformation[] { completedTask1, completedTesk2, pendingTask, newTask };

			SaveState("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[\"{2}\"]}}", completedTask1, completedTesk2, pendingTask);

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
				Mapping.New<Transformation04>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				provider.GetRunningTasks();
			}
			AssertStateSavedCorrectly("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[\"{2}\",\"{3}\"]}}", allTasks);
		}

		public void TestDeletedPendingTransformationsAreNotRemovedFromState()
		{
			var completedTask = new Transformation01();
			var pendingTask1 = new Transformation02();
			var pendingTask2 = new Transformation03();
			var pendingDeletedTask = new Transformation04();

			var allTasks = new IOnlineTransformation[] { completedTask, pendingTask1, pendingTask2, pendingDeletedTask };
			var existingTasks = new IOnlineTransformation[] { completedTask, pendingTask1, pendingTask2 };

			SaveState("{{\"Completed\":[\"{0}\"],\"Pending\":[\"{1}\",\"{2}\",\"{3}\"]}}", allTasks);

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
				Mapping.New<Transformation04>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
			}
			AssertStateSavedCorrectly("{{\"Completed\":[\"{0}\"],\"Pending\":[\"{1}\",\"{2}\",\"{3}\"]}}", allTasks);
		}

		public void TestDeletedCompletedTransformationsAreRemovedFromState()
		{
			var completedTask = new Transformation01();
			var completedDeletedTask = new Transformation02();
			var pendingTask1 = new Transformation03();
			var pendingTask2 = new Transformation04();

			var allTasks = new IOnlineTransformation[] { completedTask, completedDeletedTask, pendingTask1, pendingTask2 };
			var existingTasks = new IOnlineTransformation[] { completedTask, pendingTask1, pendingTask2 };

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
				Mapping.New<Transformation04>(new VersionLabel(9999,0)),
			});

			SaveState("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[\"{2}\",\"{3}\"]}}", allTasks);
			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				provider.GetRunningTasks();
			}
			AssertStateSavedCorrectly("{{\"Completed\":[\"{0}\"],\"Pending\":[\"{1}\",\"{2}\"]}}", existingTasks);
		}

		public void TestGetRunningTasks_WhenNoCurrentSavedState_ReturnsAllRunnableTransformations()
		{
			var newTask1 = new Transformation01();
			var newTask2 = new Transformation02();
			var newTask3 = new Transformation03();

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
			});

			var allTasks = new IOnlineTransformation[] { newTask1, newTask2, newTask3 };

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				var actualTasks = provider.GetRunningTasks();
				AssertArrayEqualsByElements(allTasks.Select(x => x.GetType()).ToArray(), actualTasks.Select(x => x.GetType()).ToArray());
			}
		}

		public void TestGetRunningTasks_OnlyNewAndPendingTasksAreReturned()
		{
			var completedTask1 = new Transformation01();
			var completedTesk2 = new Transformation02();
			var pendingTask = new Transformation03();
			var newTask = new Transformation04();

			var allTasks = new IOnlineTransformation[] { completedTask1, completedTesk2, pendingTask, newTask };

			SaveState("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[\"{2}\"]}}", completedTask1, completedTesk2, pendingTask);

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
				Mapping.New<Transformation04>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				var actualTasks = provider.GetRunningTasks();
				AssertArrayEqualsByElements(new IOnlineTransformation[] { pendingTask, newTask }.Select(x => x.GetType()).ToArray(), actualTasks.Select(x => x.GetType()).ToArray());
			}
		}

		public void TestOnTaskCompleted_SavesPendingTaskAsCompleted()
		{
			var completedTask1 = new Transformation01();
			var completedTesk2 = new Transformation02();
			var pendingTask = new Transformation03();
			var newTask = new Transformation04();

			var allTasks = new IOnlineTransformation[] { completedTask1, completedTesk2, pendingTask, newTask };

			SaveState("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[\"{2}\"]}}", completedTask1, completedTesk2, pendingTask);

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
				Mapping.New<Transformation04>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				provider.OnTaskCompleted(pendingTask);
				AssertStateSavedCorrectly("{{\"Completed\":[\"{0}\",\"{1}\",\"{2}\"],\"Pending\":[]}}", allTasks);

				provider.GetRunningTasks();
				AssertStateSavedCorrectly("{{\"Completed\":[\"{0}\",\"{1}\",\"{2}\"],\"Pending\":[\"{3}\"]}}", allTasks);

				provider.OnTaskCompleted(newTask);
				AssertStateSavedCorrectly("{{\"Completed\":[\"{0}\",\"{1}\",\"{2}\",\"{3}\"],\"Pending\":[]}}", allTasks);
			}
		}

		public void TestOnTaskCompleted_SavesNewTaskAsCompleted()
		{
			var completedTask1 = new Transformation01();
			var completedTesk2 = new Transformation02();
			var pendingTask = new Transformation03();
			var newTask = new Transformation04();

			var allTasks = new IOnlineTransformation[] { completedTask1, completedTesk2, newTask, pendingTask };

			SaveState("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[\"{2}\"]}}", completedTask1, completedTesk2, pendingTask);

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
				Mapping.New<Transformation04>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				provider.OnTaskCompleted(newTask);
				AssertStateSavedCorrectly("{{\"Completed\":[\"{0}\",\"{1}\",\"{2}\"],\"Pending\":[\"{3}\"]}}", allTasks);
			}
		}

		public void TestDeletedPendingTasksExist_ReturnsTrueWhenPendingDeletedTasksExist()
		{
			var completedTask = new Transformation01();
			var pendingTask = new Transformation02();
			var pendingDeletedTask = new Transformation03();
			var newTask = new Transformation04();

			var allTasks = new IOnlineTransformation[] { completedTask, pendingTask, newTask };

			SaveState("{{\"Completed\":[\"{0}\"],\"Pending\":[\"{1}\",\"{2}\"]}}", completedTask, pendingTask, pendingDeletedTask);

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation04>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				Assert("DeletedPendingTasksExist should return true", provider.DeletedPendingTasks.Any());
			}
		}

		public void TestDeletedPendingTasksExist_ReturnsFalseWhenNoPendingDeletedTasksExist()
		{
			var completedTask1 = new Transformation01();
			var completedTesk2 = new Transformation02();
			var pendingTask = new Transformation03();
			var newTask = new Transformation04();

			var allTasks = new IOnlineTransformation[] { completedTask1, completedTesk2, newTask, pendingTask };

			SaveState("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[\"{2}\",\"{3}\"]}}", allTasks);

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
				Mapping.New<Transformation03>(new VersionLabel(9999,0)),
				Mapping.New<Transformation04>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				AssertEquals("DeletedPendingTasksExist should return false", false, provider.DeletedPendingTasks.Any());
			}
		}

		public void TestTaskStatusIsSavedToRegistry()
		{
			var provider = new OnlineTransformationProvider();

			var status = new OnlineTransformationTaskStatus(new List<string>() { typeof(Transformation01).FullName }, new List<string>() { typeof(Transformation02).FullName });
			string expected = string.Format("{{\"Completed\":[\"{0}\"],\"Pending\":[\"{1}\"]}}", typeof(Transformation01).FullName, typeof(Transformation02).FullName);
			AssertStatusSavedCorrectly();

			status = new OnlineTransformationTaskStatus(new List<string>() { typeof(Transformation01).FullName, typeof(Transformation02).FullName }, new List<string>());
			expected = string.Format("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[]}}", typeof(Transformation01).FullName, typeof(Transformation02).FullName);
			AssertStatusSavedCorrectly();

			status = new OnlineTransformationTaskStatus(new List<string>(), new List<string>() { typeof(Transformation01).FullName, typeof(Transformation02).FullName });
			expected = string.Format("{{\"Completed\":[],\"Pending\":[\"{0}\",\"{1}\"]}}", typeof(Transformation01).FullName, typeof(Transformation02).FullName);
			AssertStatusSavedCorrectly();

			void AssertStatusSavedCorrectly()
			{
				provider.SaveTaskStatus(status);
				var actual = DataRegistry.Instance.OnlineTransformationStatus;
				AssertNotNull("OnlineDataTransformationStatus must be saved", actual);
				AssertEquals("OnlineDataTransformationStatus must be saved in the correct format", expected, actual);
			}
		}

		public void TestTaskStatusIsReadFromRegistry()
		{
			var provider = new OnlineTransformationProvider();
			var factory = new BusinessObjectFactory();

			var serialized = string.Format("{{\"Completed\":[\"{0}\"],\"Pending\":[\"{1}\"]}}", typeof(Transformation01).FullName, typeof(Transformation02).FullName);
			var expectedCompleted = new List<string>() { typeof(Transformation01).FullName };
			var expectedPending = new List<string>() { typeof(Transformation02).FullName };
			AssertStatusReadCorrectly();

			serialized = string.Format("{{\"Completed\":[\"{0}\",\"{1}\"],\"Pending\":[]}}", typeof(Transformation01).FullName, typeof(Transformation02).FullName);
			expectedCompleted = new List<string>() { typeof(Transformation01).FullName, typeof(Transformation02).FullName };
			expectedPending = new List<string>();
			AssertStatusReadCorrectly();

			serialized = string.Format("{{\"Completed\":[],\"Pending\":[\"{0}\",\"{1}\"]}}", typeof(Transformation01).FullName, typeof(Transformation02).FullName);
			expectedCompleted = new List<string>();
			expectedPending = new List<string>() { typeof(Transformation01).FullName, typeof(Transformation02).FullName };
			AssertStatusReadCorrectly();

			void AssertStatusReadCorrectly()
			{
				DataRegistry.Instance.OnlineTransformationStatus = serialized;

				var status = provider.ReadTaskStatus();

				AssertEquals(expectedCompleted.Count, status.Completed.Count());
				expectedCompleted.ForEach(t => AssertCollectionContains($"Completed tasks must contain {t}", t, status.Completed));
				AssertEquals(expectedPending.Count, status.Pending.Count());
				expectedPending.ForEach(t => AssertCollectionContains($"Completed tasks must contain {t}", t, status.Pending));
			}
		}

		public void TestGetRunningTasks()
		{
			var transform1 = new Transformation01();
			var transform2 = new Transformation02();
			var allTransformations = new IOnlineTransformation[] { transform1, transform2 };

			DataRegistry.Instance.OnlineTransformationStatus = null;

			var mockMappingProvider = new Mock<ITransformationMappingProvider>();
			mockMappingProvider.Setup(x => x.GetAllMappings()).Returns(new[]
			{
				Mapping.New<Transformation01>(new VersionLabel(9999,0)),
				Mapping.New<Transformation02>(new VersionLabel(9999,0)),
			});

			using (ObjectFactory.Substitute(mockMappingProvider.Object))
			{
				var provider = new OnlineTransformationProvider();
				var runningTasks = provider.GetRunningTasks();
				AssertArrayEqualsByElements("All transformation tasks must be returned", allTransformations.Select(x => x.GetType()).ToArray(), runningTasks.Select(x => x.GetType()).ToArray());
			}
		}

		#region Implementation

		internal static void SaveState(string stateFormat, params IOnlineTransformation[] tasks)
		{
			var factory = new BusinessObjectFactory();
			var state = string.Format(stateFormat, tasks);
			DataRegistry.Instance.OnlineTransformationStatus = state;
		}

		void EnsureNoState()
		{
			DataRegistry.Instance.OnlineTransformationStatus = null;
		}

		internal static void AssertStateSavedCorrectly(string expectedFormat, params IOnlineTransformation[] allTasks)
		{
			var status = DataRegistry.Instance.OnlineTransformationStatus;
			AssertNotNull("OnlineDataTransformationStatus must be saved", status);
			AssertEquals("OnlineDataTransformationStatus must be saved in the correct format", string.Format(expectedFormat, allTasks), status);
		}

		#endregion
	}
}
