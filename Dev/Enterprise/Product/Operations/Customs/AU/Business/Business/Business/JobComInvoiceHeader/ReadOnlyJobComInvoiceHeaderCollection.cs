//using System;
//
//using Enterprise.ZArchitecture;
//
//using NUnit.Framework;
//
//namespace Enterprise.Customs.AU.Declaration.Business
//{
//	public class ReadOnlyJobComInvoiceHeaderCollection : JobComInvoiceHeaderCollection
//	{
//		public ReadOnlyJobComInvoiceHeaderCollection(JobDeclaration JobDeclaration) : base(JobDeclaration)
//		{
//		}
//
//		public ReadOnlyJobComInvoiceHeaderCollection(JobComInvoiceGroupHeader ParentJobComInvoiceGroupHeader) : base(ParentJobComInvoiceGroupHeader)
//		{
//		}
//
//		public override JobComInvoiceHeader AddNew()
//		{
//			throw new InvalidOperationException("This ia a read only collection.  Try adding your record through the group header");
//		}
//		
//	}
//}
//
//#if DEBUG
//namespace Enterprise.Customs.AU.Declaration.Business.Testing
//{
//	using NUnit.Framework;
//	using Enterprise.ZArchitecture.Business.Testing;
//
//	public class ReadOnlyJobComInvoiceHeaderCollectionTest : TestCaseWithFactory
//	{
//		
//
//		[ExpectException(typeof(InvalidOperationException))]
//		public void TestReadOnlyJobComInvoiceHeaderCollection()
//		{
//			JobDeclaration JobDeclaration = Factory.New<JobDeclaration>();
//			ReadOnlyJobComInvoiceHeaderCollection ReadOnlyCollection  = new ReadOnlyJobComInvoiceHeaderCollection(JobDeclaration);
//
//			ReadOnlyCollection.AddNew();
//		}
//	}
//}
//#endif
