using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	sealed class DocumentParserTest : TestCaseWithFactory
	{
		readonly string StartTag = Constants.DocumentEngine.EmailParsing.StartTag;
		readonly string EndTag = Constants.DocumentEngine.EmailParsing.EndTag;
		const string DocumentIsMalformedMessage = "The attached document is malformed, a start tag {0} should always be followed by an end tag {1}";
		const string NoDocWrapperForBusinessObjectMessage = "Business Object of type {0} does not have a document wrapper of type {1}";

		#region TestNew

		public void TestNew()
		{
			DummyDocumentParser.RegisterThisSubTypeOverride();

			try
			{
				AssertEquals(true, DocumentParser.New(null, Factory) is DummyDocumentParser);
			}
			finally
			{
				DummyDocumentParser.UnregisterThisSubTypeOverride();
			}
		}

		#endregion

		#region Test Objects

		public class DocumentParserForTest : DocumentParser<BusinessObject>
		{
			public DocumentParserForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type TypeOfWrapper
			{
				get { return typeof(WrapperClass); }
			}
		}

		public class DocumentParserForTest2 : DocumentParser<BusinessObject>
		{
			public DocumentParserForTest2(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type TypeOfWrapper
			{
				get { return typeof(WrapperClass2); }
			}
		}

		public class DocumentParserForTest3 : DocumentParser<BusinessObject>
		{
			public DocumentParserForTest3(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type TypeOfWrapper
			{
				get { return typeof(WrapperClass); }
			}

			protected override ZString ParseCore(BusinessObject objectToWrap, ZString documentText)
			{
				DocBuilderParsingRoots = new BusinessObject[] { GetDocWrapper(objectToWrap, Factory) };
				return base.ParseCore(objectToWrap, documentText);
			}
		}

		public class WrappedClass : DummyBusinessObject
		{
			public WrappedClass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString Orange
			{
				get { return "Orange"; }
			}

			public ZString Apple
			{
				get { return "I like Apples"; }
			}

			public ZString AppleTag
			{
				get { return "(*Apple"; }
			}

			public ZString Bananna
			{
				get { return "Bananna*)"; }
			}

			public ZString Pineapple
			{
				get { return "(*Pineapple*)"; }
			}

			public WrappedClass RelatedObject
			{
				get
				{
					return reelatedObject ?? (reelatedObject = Factory.New<WrappedClass>());
				}
			}
			WrappedClass reelatedObject;
		}

		public class WrapperClass : DocumentWrapper
		{
			protected WrapperClass(WrappedClass objectToBeWrapped, BusinessObjectFactory factory)
			{
				this.ObjectToBeWrapped = objectToBeWrapped;
			}

			protected WrappedClass ObjectToBeWrapped;

			public static WrapperClass New(WrappedClass objectToBeWrapped, BusinessObjectFactory factory)
			{
				return new WrapperClass(objectToBeWrapped, factory);
			}

			[DocumentField("All about Apples version 1")]
			public ZString Apple
			{
				get { return ObjectToBeWrapped.Apple; }
			}

			[DocumentField("AppleTag contains a tag")]
			public ZString AppleTag
			{
				get { return ObjectToBeWrapped.AppleTag; }
			}

			[DocumentField("Bananna contains a end tag")]
			public ZString Bananna
			{
				get { return ObjectToBeWrapped.Bananna; }
			}

			[DocumentField("Pineapple contains a start and a end tag")]
			public ZString Pineapple
			{
				get { return ObjectToBeWrapped.Pineapple; }
			}

			[DocumentField("Find apple")]
			public ZString GetApple()
			{
				return Apple;
			}

			[DocumentField("Find fruit by name")]
			public ZString GetFruit(ZString fruitName, ZBool upperCase)
			{
				var result = fruitName == "Apple" ? Apple : Pineapple;
				return upperCase ? result.ToUpper() : result;
			}

			[DocumentField("Find fruit by name")]
			public ZString GetFruit(ZString fruitName)
			{
				var result = fruitName == "Apple" ? Apple : Pineapple;
				return result;
			}

			[DocumentField("Like fruit by name")]
			public ZString ILikeFruit(ZString fruitName)
			{
				return "I LIKE " + fruitName;
			}

			[DocumentField("Related document wrapper")]
			public WrapperClass RelatedDocumentWrapper
			{
				get { return relatedDocumentWrapper ?? (relatedDocumentWrapper = WrapperClass.New(ObjectToBeWrapped, ObjectToBeWrapped.Factory)); }
			}
			WrapperClass relatedDocumentWrapper;

			[DocumentField("Get related document wrapper")]
			public WrapperClass GetRelatedDocumentWrapper()
			{
				return RelatedDocumentWrapper;
			}

			[DocumentField("Related Business Object")]
			public WrappedClass RelatedBusinessObject
			{
				get { return ObjectToBeWrapped.RelatedObject; }
			}

			[DocumentField("Get related business object")]
			public WrappedClass GetRelatedBusinessObject()
			{
				return ObjectToBeWrapped.RelatedObject;
			}

			public ZString HiddenFruit
			{
				get { return "Peach"; }
			}

			public override string ToString()
			{
				return "";
			}
		}

		public class WrapperClass2 : WrapperClass
		{
			protected WrapperClass2(WrappedClass objectToBeWrapped, BusinessObjectFactory factory)
				: base(objectToBeWrapped, factory)
			{
			}

			public new static WrapperClass2 New(WrappedClass objectToBeWrapped, BusinessObjectFactory factory)
			{
				return new WrapperClass2(objectToBeWrapped, factory);
			}

			[DocumentField("All about Apples")]
			public ZString Apple2
			{
				get { return ObjectToBeWrapped.Apple; }
			}
		}

		#endregion

		#region Parse

		public void TestParseWithInheritedDocWrapper()
		{
			var objectToBeWrapped = Factory.New<WrappedClass>();
			var parser = new DocumentParserForTest2(Factory);

			var parsedText = parser.Parse(objectToBeWrapped, "Parse me (*Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*Apple2*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);
		}

		public void TestParse()
		{
			var objectToBeWrapped = Factory.New<WrappedClass>();
			var parser = new DocumentParserForTest(Factory);

			var parsedText = parser.Parse(objectToBeWrapped, "Parse me Apple");
			AssertEquals("Tag without tag definitions does not get replaced", "Parse me Apple", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*Apple*) more (*Apple*) more (*Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples more I like Apples more I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*<span class=SpellE>Apple</span>*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, @"Parse me (*<span 
class=SpellE>Apple</span>*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me <span class=SpellE>Something</span>");
			AssertEquals("Text should be", "Parse me <span class=SpellE>Something</span>", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*Apple*) (*Orange*) (*Apple*) (*Orange*)");
			AssertEquals("Text should be", "Parse me I like Apples (*Orange*) I like Apples (*Orange*)", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*AppleTag*) (*Orange*) (*Apple*) (*Bananna*)");
			AssertEquals("Text should be", "Parse me (*Apple (*Orange*) I like Apples Bananna*)", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*Pineapple*)");
			AssertEquals("Text should be", "Parse me (*Pineapple*)", parsedText);

			string exceptionMsg = "";

			try
			{
				parser.Parse(objectToBeWrapped, "Parse me " + EndTag + "Apple" + StartTag);
			}
			catch (DocumentParsingFailedException e)
			{
				exceptionMsg = e.Message;
			}

			AssertEquals("Exception Message should be", String.Format(DocumentIsMalformedMessage, StartTag, EndTag), exceptionMsg);
		}

		public void TestParse_Method()
		{
			var objectToBeWrapped = Factory.New<WrappedClass>();
			var parser = new DocumentParserForTest(Factory);

			string parsedText = "";

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetApple()*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetFruit(\"Apple\", false)*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetFruit(Apple, false)*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetFruit(\"Apple\", true)*)");
			AssertEquals("Text should be", "Parse me I LIKE APPLES", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetFruit(\"App\", false)*)");
			AssertEquals("Text should be", "Parse me (*Pineapple*)", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetFruit(\"App\", true)*)");
			AssertEquals("Text should be", "Parse me (*PINEAPPLE*)", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetFruit(\"A,p,p\", true)*)");
			AssertEquals("Text should be", "Parse me (*PINEAPPLE*)", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetFruit(Apple)*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetFruit(\"Apple()\",\"true\")*)");
			AssertEquals("Should cope with brackets inside quotes", "Parse me (*PINEAPPLE*)", parsedText);
		}

		public void TestParse_RelatedDocumentWrapper()
		{
			var objectToBeWrapped = Factory.New<WrappedClass>();
			var parser = new DocumentParserForTest(Factory);

			string parsedText = "";

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*RelatedDocumentWrapper.Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*RelatedDocumentWrapper.Pineapple*)");
			AssertEquals("Text should be", "Parse me (*Pineapple*)", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*RelatedDocumentWrapper.GetFruit(\"Apple\", true)*)");
			AssertEquals("Text should be", "Parse me I LIKE APPLES", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*RelatedDocumentWrapper.RelatedDocumentWrapper.Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetRelatedDocumentWrapper().Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetRelatedDocumentWrapper().RelatedDocumentWrapper.Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetRelatedDocumentWrapper().GetRelatedDocumentWrapper().GetApple()*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*RelatedDocumentWrapper*)");
			AssertEquals("Text should be", "Parse me ", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetRelatedDocumentWrapper()*)");
			AssertEquals("Text should be", "Parse me ", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetRelatedDocumentWrapper().*)");
			AssertEquals("Text should be", "Parse me ", parsedText);
		}

		public void TestParse_RelatedBusinessObject()
		{
			var objectToBeWrapped = Factory.New<WrappedClass>();
			var parser = new DocumentParserForTest(Factory);

			string parsedText = "";

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*RelatedBusinessObject.Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*RelatedBusinessObject.Pineapple*)");
			AssertEquals("Text should be", "Parse me (*Pineapple*)", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*RelatedBusinessObject.RelatedObject.Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetRelatedBusinessObject().Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*GetRelatedBusinessObject().RelatedObject.Apple*)");
			AssertEquals("Text should be", "Parse me I like Apples", parsedText);
		}

		public void TestParse_NestedTags()
		{
			var objectToBeWrapped = Factory.New<WrappedClass>();
			var parser = new DocumentParserForTest(Factory);

			string parsedText = "";

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*ILikeFruit(\"(*Apple*)\")*).");
			AssertEquals("Text should be", "Parse me I LIKE I like Apples.", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*ILikeFruit(\"(*GetFruit(\"Apple()\",\"true\")*)\")*).");
			AssertEquals("Text should be", "Parse me I LIKE (*PINEAPPLE*).", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*IF('(*GetApple()*)'=='I like Apples', 'Y', 'N')*)");
			AssertEquals("Text should be", "Parse me (*IF('I like Apples'=='I like Apples', 'Y', 'N')*)", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*IF('(*GetApple()*)'=='I like Apples', 'Y', 'N')*) (*GetApple()*)");
			AssertEquals("Text should be", "Parse me (*IF('I like Apples'=='I like Apples', 'Y', 'N')*) I like Apples", parsedText);

			ZString exceptionMsg = ZString.Empty;

			try
			{
				parsedText = parser.Parse(objectToBeWrapped, "Parse me (*aaa (*bb(*Apple*)");
			}
			catch (DocumentParsingFailedException e)
			{
				exceptionMsg = e.Message;
			}
			AssertEquals("Exception Message should be", String.Format(DocumentIsMalformedMessage, StartTag, EndTag), exceptionMsg);

			try
			{
				parsedText = parser.Parse(objectToBeWrapped, "Parse me (*Apple*)aaa*)bbb*)");
			}
			catch (DocumentParsingFailedException e)
			{
				exceptionMsg = e.Message;
			}
			AssertEquals("Exception Message should be", String.Format(DocumentIsMalformedMessage, StartTag, EndTag), exceptionMsg);

			try
			{
				parsedText = parser.Parse(objectToBeWrapped, "Parse me (*Apple*) aaa*)bbb(*");
			}
			catch (DocumentParsingFailedException e)
			{
				exceptionMsg = e.Message;
			}
			AssertEquals("Exception Message should be", String.Format(DocumentIsMalformedMessage, StartTag, EndTag), exceptionMsg);
		}

		public void TestParse_UseDocBuilderParser()
		{
			var objectToBeWrapped = Factory.New<WrappedClass>();
			var parser = new DocumentParserForTest3(Factory);

			string parsedText = "";

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*HiddenFruit*)");
			AssertEquals("Text should be", "Parse me Peach", parsedText);

			parsedText = parser.Parse(objectToBeWrapped, "Parse me (*IF(\"(*GetApple()*)\"==\"I like Apples\", \"YES\", \"NO\")*)");
			AssertEquals("Text should be", "Parse me YES", parsedText);
		}

		public void TestParseWithObjectThatDoesNotMatchWrapperType()
		{
			var parser = new DocumentParserForTest(Factory);
			var objectWithDifferentWrapper = Factory.New<DummyBusinessObject>();
			ZString exceptionMsg = ZString.Empty;

			try
			{
				parser.Parse(objectWithDifferentWrapper, "Parse me " + StartTag + "Apple" + EndTag);
			}
			catch (DeveloperNotificationException e)
			{
				exceptionMsg = e.Message;
			}

			AssertEquals("Exception Message should be", string.Format(NoDocWrapperForBusinessObjectMessage, objectWithDifferentWrapper.GetType(), typeof(WrapperClass)), exceptionMsg);
		}

		#endregion
	}
}
