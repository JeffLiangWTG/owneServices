using System;

namespace Enterprise.Customs.AE.Business.Testing;

public class RootLineTest : ManifestLineTest.ManifestLineTesting
{
	public void TestRecordIdentifier()
	{
		Assert(string.IsNullOrEmpty(new RootLine().RecordIdentifier));
	}

	public void TestRootLine()
	{
		RootLine root = new RootLine();
		Assert(root.IsEmpty);
		AssertEquals(0, root.FieldCount);
		AssertNotNull(root.Children);
	}

	#region override
	protected override ManifestLine GetNewManifestLine()
	{
		return new RootLine();
	}

	protected override int FieldCount
	{
		get
		{
			return 0;
		}
	}

	protected override Type ExpectedType
	{
		get
		{
			return typeof(RootLine);
		}
	}
	#endregion
}
