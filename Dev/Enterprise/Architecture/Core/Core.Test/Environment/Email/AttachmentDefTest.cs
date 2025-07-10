using System.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AttachmentDefTest : TestCase
	{
		public void TestCreateZippedAttachmentByFileName()
		{
			string contents = new string('x', 10000);
			using (TempFile tempFile = TempFile.New())
			{
				File.WriteAllText(tempFile.Filename, contents);
				AttachmentDef def = AttachmentDef.CreateZippedAttachment("DisplayName.ZIP", tempFile.Filename);
				AssertEquals("DisplayName.ZIP", def.DisplayName);
				Assert(def.Data.Length > 0);
				Assert(def.Data.Length < contents.Length);
			}
		}
	}
}
