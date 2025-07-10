using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	public class TestDocumentPrintedEventArgs : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var args = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, Factory.New<IStmMenuItem>());
			AssertNotNull(args.MenuItem);
			AssertEquals(DeliveryInstructionDestination.Print, args.DeliveryInstructionDestinationType);
			AssertEquals(expected: false, args.IsDraft);
			AssertEquals(null, args.Source);

			var args1 = new DocumentPrintedEventArgs(DeliveryInstructionDestination.DocManager, Factory.New<IStmMenuItem>(), true);
			AssertNotNull(args1.MenuItem);
			AssertEquals(DeliveryInstructionDestination.DocManager, args1.DeliveryInstructionDestinationType);
			AssertEquals(expected: true, args1.IsDraft);
			AssertEquals(null, args1.Source);

			var args2 = new DocumentPrintedEventArgs(DeliveryInstructionDestination.DocManager, Factory.New<IStmMenuItem>(), true, new object());
			AssertNotNull(args2.MenuItem);
			AssertEquals(DeliveryInstructionDestination.DocManager, args2.DeliveryInstructionDestinationType);
			AssertEquals(expected: true, args2.IsDraft);
			AssertNotNull(args2.Source);
		}
	}
}
