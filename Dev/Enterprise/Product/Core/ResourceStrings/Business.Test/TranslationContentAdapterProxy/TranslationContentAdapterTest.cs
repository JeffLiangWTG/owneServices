using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class TranslationContentAdapterTest : TransactionedTestCase
	{
		public void TestExport()
		{
			using (var dir = new TempDirectory())
			{
				var adapter = new TranslationContentAdapterForTest("MyAccountLearningCentre");
				adapter.Export(dir.DirectoryName);

				string path1 = Path.Combine(dir.DirectoryName, "name1___.xml");
				string path2 = Path.Combine(dir.DirectoryName, "_reservedword_.xml");

				Assert(File.Exists(path1));
				Assert(File.Exists(path2));

				AssertEquals("<EnterpriseResources Language=\"ENG\">" + @"
  <Res>
    <Key>key1</Key>
    <Caption>value1</Caption>
  </Res>
  <Res>
    <Key>key2</Key>
    <Caption>value2</Caption>
  </Res>
</EnterpriseResources>", File.ReadAllText(path1));

				AssertEquals("<EnterpriseResources Language=\"ENG\">" + @"
  <Res>
    <Key>key1</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>", File.ReadAllText(path2));
			}
		}

		public void TestImport()
		{
			using (var dir = new TempDirectory())
			using (var file1 = TempFile.New(dir.DirectoryName, "xml"))
			using (var file2 = TempFile.New(dir.DirectoryName, "xml"))
			{
				File.WriteAllText(file1.Filename, "<EnterpriseResources Language=\"ENG\">" + @"
  <Res>
    <Key>key3</Key>
    <Caption>value3</Caption>
  </Res>  
</EnterpriseResources>");

				File.WriteAllText(file2.Filename, "<EnterpriseResources Language=\"ENG\">" + @"
  <Res>
    <Key>key4</Key>
    <Caption>value4</Caption>
  </Res>  
</EnterpriseResources>");

				var adapter = new TranslationContentAdapterForTest("MyAccountLearningCentre");
				adapter.Import("UKR", dir.DirectoryName);
				AssertEquals(2, adapter.Requests.Count);
			}
		}

		public void TestRetry()
		{
			using (var dir = new TempDirectory())
			{
				var adapter = new TranslationContentAdapterForTest("MyAccountLearningCentre");
				adapter.FailTimes = 1;
				adapter.Export(dir.DirectoryName);

				string path1 = Path.Combine(dir.DirectoryName, "name1___.xml");
				string path2 = Path.Combine(dir.DirectoryName, "_reservedword_.xml");

				Assert(File.Exists(path1));
				Assert(File.Exists(path2));

				AssertEquals(2, adapter.TriesCount);
			}
		}

		public void TestRetry_NoInfinite()
		{
			using (var dir = new TempDirectory())
			{
				var adapter = new TranslationContentAdapterForTest("MyAccountLearningCentre");
				adapter.FailTimes = 10;
				AssertExceptionThrown(typeof(TaskCanceledException), "poof", delegate
				{ adapter.Export(dir.DirectoryName); });

				string path1 = Path.Combine(dir.DirectoryName, "name1___.xml");
				string path2 = Path.Combine(dir.DirectoryName, "_reservedword_.xml");

				Assert(!File.Exists(path1));
				Assert(!File.Exists(path2));
				AssertEquals(2, adapter.TriesCount);
			}
		}

		class TranslationContentAdapterForTest : TranslationContentAdapter
		{
			public TranslationContentAdapterForTest(string contentModule) : base(contentModule) { }

			public int FailTimes;
			public int TriesCount { get; private set; }
			protected override Dictionary<string, string> GetContentsCore(string language)
			{
				TriesCount++;

				if (FailTimes > 0)
				{
					FailTimes--;
					throw new TaskCanceledException("poof", new TimeoutException());
				}

				var dic = new Dictionary<string, string>();
				dic.Add("name1\\/:", "<EnterpriseResources Language=\"ENG\">" + @"
  <Res>
    <Key>key1</Key>
    <Caption>value1</Caption>
  </Res>
  <Res>
    <Key>key2</Key>
    <Caption>value2</Caption>
  </Res>
</EnterpriseResources>");

				dic.Add("LPT1", "<EnterpriseResources Language=\"ENG\">" + @"
  <Res>
    <Key>key1</Key>
    <Caption>value4</Caption>
  </Res>
</EnterpriseResources>");

				return dic;
			}

			readonly List<StringContent> requests = new List<StringContent>();
			protected override void ImportCore(HttpClient client, StringContent requestContent)
			{
				requests.Add(requestContent);
			}

			public List<StringContent> Requests { get { return requests; } }
		}
	}
}
