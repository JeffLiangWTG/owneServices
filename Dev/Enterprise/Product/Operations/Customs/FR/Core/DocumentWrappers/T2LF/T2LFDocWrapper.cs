using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit;

[CodeAlive("Used in documents DataContext")]
[DefaultField(nameof(HumanReadableName)), WrapperTypeName("T2LF")]
public class T2LFDocWrapper : DocBaseWrapper
{
	T2LFDocWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory, bool isForT2LF) : base(entryHeader, factory)
	{
		if (entryHeader != null)
		{
			t2LDocLines = new T2LDocLineWrapperCollection(EntryHeader.T2LFApplicableEntryLines, Factory, isForT2LF);
		}
	}

	public static T2LFDocWrapper New(CusEntryHeader entryHeader, BusinessObjectFactory factory) => new T2LFDocWrapper(entryHeader, factory, true);

	public CusEntryHeader EntryHeader => (CusEntryHeader)WrappedObject;

	public T2LDocLineWrapperCollection T2LDocLines => t2LDocLines;
	public T2LDocLineWrapperCollection t2LDocLines;
}
