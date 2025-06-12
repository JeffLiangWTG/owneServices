using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class XHubMessageHeaderTest
	{
		[TestMethod]
		public void TestMessageHeader()
		{
			var property1 = new Property {Name = "SenderID", Namespace = "namespace", Value = "SENDER"};
			var property2 = new Property {Name = "RecipientID", Namespace = "namespace", Value = "RECEIPT"};
			
			var header = new XHubMessageHeader(new[] {property1, property2});
			Assert.AreEqual("XHubContext", header.Name);
			Assert.AreEqual(String.Empty, header.Namespace);
		}
	}
}
