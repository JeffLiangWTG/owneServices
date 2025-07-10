using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class AggregatedSourceControlTest : TestCase
	{
		public void TestAddFile()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.AddFile("Q:\\Foo\\Bar\\File.txt")).Returns(true).Verifiable();
			mock2.Setup(x => x.AddFile("q:\\foo\\baz\\file.txt")).Returns(false).Verifiable();

			AssertEquals(true, agg.AddFile("Q:\\Foo\\Bar\\File.txt"));
			AssertEquals(false, agg.AddFile("q:\\foo\\baz\\file.txt"));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestDeleteFile()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.DeleteFile("Q:\\Foo\\Bar\\File.txt")).Returns(false).Verifiable();
			mock2.Setup(x => x.DeleteFile("q:\\foo\\baz\\file.txt")).Returns(true).Verifiable();

			AssertEquals(false, agg.DeleteFile("Q:\\Foo\\Bar\\File.txt"));
			AssertEquals(true, agg.DeleteFile("q:\\foo\\baz\\file.txt"));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestDeleteDirectory()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.DeleteDirectory("Q:\\Foo\\Bar\\Directory")).Returns(true).Verifiable();
			mock2.Setup(x => x.DeleteDirectory("q:\\foo\\baz\\directory")).Returns(false).Verifiable();

			AssertEquals(true, agg.DeleteDirectory("Q:\\Foo\\Bar\\Directory"));
			AssertEquals(false, agg.DeleteDirectory("q:\\foo\\baz\\directory"));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestCheckout()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.CheckOut("Q:\\Foo\\Bar\\File.txt", true)).Verifiable();
			mock2.Setup(x => x.CheckOut("q:\\foo\\baz\\file.txt", false)).Verifiable();

			agg.CheckOut("Q:\\Foo\\Bar\\File.txt", true);
			agg.CheckOut("q:\\foo\\baz\\file.txt", false);

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestCheckout_Multi()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.CheckOut(new[] { "Q:\\Foo\\Bar\\File.txt", "Q:\\Foo\\Bar\\OtherFile.txt" }, false)).Verifiable();
			mock2.Setup(x => x.CheckOut(new[] { "q:\\foo\\baz\\file.txt", "q:\\foo\\baz\\Otherfile.txt" }, false)).Verifiable();

			agg.CheckOut(new[] { "Q:\\Foo\\Bar\\File.txt", "q:\\foo\\baz\\file.txt", "Q:\\Foo\\Bar\\OtherFile.txt", "q:\\foo\\baz\\Otherfile.txt" }, false);

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestIsDifferent()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.CheckOut("Q:\\Foo\\Bar\\File.txt", true)).Verifiable();
			mock2.Setup(x => x.CheckOut("q:\\foo\\baz\\file.txt", false)).Verifiable();

			agg.CheckOut("Q:\\Foo\\Bar\\File.txt", true);
			agg.CheckOut("q:\\foo\\baz\\file.txt", false);

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestIsFileCheckedOutByMe()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.IsFileCheckedOutByMe("Q:\\Foo\\Bar\\File.txt")).Returns(true).Verifiable();
			mock2.Setup(x => x.IsFileCheckedOutByMe("q:\\foo\\baz\\file.txt")).Returns(false).Verifiable();

			AssertEquals(true, agg.IsFileCheckedOutByMe("Q:\\Foo\\Bar\\File.txt"));
			AssertEquals(false, agg.IsFileCheckedOutByMe("q:\\foo\\baz\\file.txt"));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestIsFileInSourceControl()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.IsFileInSourceControl("Q:\\Foo\\Bar\\File.txt")).Returns(false).Verifiable();
			mock2.Setup(x => x.IsFileInSourceControl("q:\\foo\\baz\\file.txt")).Returns(true).Verifiable();

			AssertEquals(false, agg.IsFileInSourceControl("Q:\\Foo\\Bar\\File.txt"));
			AssertEquals(true, agg.IsFileInSourceControl("q:\\foo\\baz\\file.txt"));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestIsFolderInSourceControl()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.IsFolderInSourceControl("Q:\\Foo\\Bar\\File.txt")).Returns(true).Verifiable();
			mock2.Setup(x => x.IsFolderInSourceControl("q:\\foo\\baz\\file.txt")).Returns(false).Verifiable();

			AssertEquals(true, agg.IsFolderInSourceControl("Q:\\Foo\\Bar\\File.txt"));
			AssertEquals(false, agg.IsFolderInSourceControl("q:\\foo\\baz\\file.txt"));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestUndoCheckout()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.UndoCheckOut("Q:\\Foo\\Bar\\File.txt", true)).Verifiable();
			mock2.Setup(x => x.UndoCheckOut("q:\\foo\\baz\\file.txt", false)).Verifiable();

			agg.UndoCheckOut("Q:\\Foo\\Bar\\File.txt", true);
			agg.UndoCheckOut("q:\\foo\\baz\\file.txt", false);

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestUndoCheckout_Multi()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.UndoCheckOut(new[] { "Q:\\Foo\\Bar\\File.txt", "Q:\\Foo\\Bar\\OtherFile.txt" }, false)).Verifiable();
			mock2.Setup(x => x.UndoCheckOut(new[] { "q:\\foo\\baz\\file.txt", "q:\\foo\\baz\\Otherfile.txt" }, false)).Verifiable();

			agg.UndoCheckOut(new[] { "Q:\\Foo\\Bar\\File.txt", "q:\\foo\\baz\\file.txt", "Q:\\Foo\\Bar\\OtherFile.txt", "q:\\foo\\baz\\Otherfile.txt" }, false);

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestPendAdd()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.PendAdd("Q:\\Foo\\Bar\\File.txt")).Verifiable();
			mock2.Setup(x => x.PendAdd("q:\\foo\\baz\\file.txt")).Verifiable();

			agg.PendAdd("Q:\\Foo\\Bar\\File.txt");
			agg.PendAdd("q:\\foo\\baz\\file.txt");

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestPendAdd_Multi()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.PendAdd("Q:\\Foo\\Bar\\File.txt")).Verifiable();
			mock2.Setup(x => x.PendAdd("q:\\foo\\baz\\file.txt")).Verifiable();

			agg.PendAdd("Q:\\Foo\\Bar\\File.txt");
			agg.PendAdd("q:\\foo\\baz\\file.txt");

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestPendEdit()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.PendEdit("Q:\\Foo\\Bar\\File.txt")).Verifiable();
			mock2.Setup(x => x.PendEdit("q:\\foo\\baz\\file.txt")).Verifiable();

			agg.PendEdit("Q:\\Foo\\Bar\\File.txt");
			agg.PendEdit("q:\\foo\\baz\\file.txt");

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestSubmitChanges()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);
			var items = new[] { "item1", "item2" };

			mock1.Setup(x => x.SubmitChanges("SCO", "name", "criticality", items)).Verifiable();
			mock2.Setup(x => x.SubmitChanges("SCO", "name", "criticality", items)).Verifiable();

			agg.SubmitChanges("SCO", "name", "criticality", items);

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestGetFilesWithPendingChanges()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.GetFilesWithPendingChanges(false)).Returns(new[] { "Q:\\Foo\\Bar\\File.txt" }).Verifiable();
			mock2.Setup(x => x.GetFilesWithPendingChanges(false)).Returns(new[] { "Q:\\Foo\\Baz\\File.txt" }).Verifiable();

			AssertContainsExactElementsInAnyOrder(new[] { "Q:\\Foo\\Bar\\File.txt", "Q:\\Foo\\Baz\\File.txt" }, agg.GetFilesWithPendingChanges(false));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestGetCurrentBranchName()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.GetCurrentBranchName()).Returns("Foobar").Verifiable();

			AssertEquals("Foobar", agg.GetCurrentBranchName());

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestDispose()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();

			using (var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object))
			{
				mock1.Setup(x => x.Dispose()).Verifiable();
				mock2.Setup(x => x.Dispose()).Verifiable();
			}

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestGetFilesWithChangesInCurrentBranch()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var mockReleaseInfo = repository.Create<IReleaseInfo>();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);

			mock1.Setup(x => x.GetFilesWithChangesInCurrentBranch(mockReleaseInfo.Object, false)).Returns(new[] { "Q:\\Foo\\Bar\\File.txt" }).Verifiable();
			mock2.Setup(x => x.GetFilesWithChangesInCurrentBranch(mockReleaseInfo.Object, false)).Returns(new[] { "Q:\\Foo\\Baz\\File.txt" }).Verifiable();

			AssertContainsExactElementsInAnyOrder(new[] { "Q:\\Foo\\Bar\\File.txt", "Q:\\Foo\\Baz\\File.txt" }, agg.GetFilesWithChangesInCurrentBranch(mockReleaseInfo.Object, false));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestNewWithDuplicateRepository()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock = repository.Create<IAggregatableSourceControl>();

			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock.Object, "Q:\\foo\\bar", mock.Object);

			AssertSame(agg, mock.Object);
		}

		public void TestNewWithDuplicatePath()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();

			mock2.Setup(x => x.Dispose()).Verifiable();
			var agg = AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\foo\\bar", mock2.Object);

			AssertSame(agg, mock1.Object);
			AssertNoExceptionThrown(() => repository.VerifyAll());
		}

		public void TestWithAdditionalRepository()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var mock3 = repository.Create<IAggregatableSourceControl>();

			var agg = (AggregatedSourceControl)AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);
			AssertContainsExactElementsInExactOrder(new[] { mock1.Object, mock2.Object }, agg.Children.Select(x => x.Value));

			agg = (AggregatedSourceControl)agg.WithAdditionalRepository(mock3.Object, "Q:\\Foo\\Qux");
			AssertContainsExactElementsInExactOrder(new[] { mock1.Object, mock2.Object, mock3.Object }, agg.Children.Select(x => x.Value));
		}

		public void TestWithDuplicateRepository()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();

			var agg = (AggregatedSourceControl)AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);
			AssertContainsExactElementsInExactOrder(new[] { mock1.Object, mock2.Object }, agg.Children.Select(x => x.Value));

			agg = (AggregatedSourceControl)agg.WithAdditionalRepository(mock2.Object, "Q:\\Foo\\baz");
			AssertContainsExactElementsInExactOrder(new[] { mock1.Object, mock2.Object }, agg.Children.Select(x => x.Value));
		}

		public void TestWithDuplicatePath()
		{
			var repository = new MockRepository(MockBehavior.Strict);

			var mock1 = repository.Create<IAggregatableSourceControl>();
			var mock2 = repository.Create<IAggregatableSourceControl>();
			var mock3 = repository.Create<IAggregatableSourceControl>();

			var agg = (AggregatedSourceControl)AggregatedSourceControl.New("Q:\\Foo\\Bar", mock1.Object, "Q:\\Foo\\Baz", mock2.Object);
			AssertContainsExactElementsInExactOrder(new[] { mock1.Object, mock2.Object }, agg.Children.Select(x => x.Value));

			mock3.Setup(x => x.Dispose()).Verifiable();

			agg = (AggregatedSourceControl)agg.WithAdditionalRepository(mock3.Object, "Q:\\Foo\\baz");
			AssertContainsExactElementsInExactOrder(new[] { mock1.Object, mock2.Object }, agg.Children.Select(x => x.Value));

			AssertNoExceptionThrown(() => repository.VerifyAll());
		}
	}
}
