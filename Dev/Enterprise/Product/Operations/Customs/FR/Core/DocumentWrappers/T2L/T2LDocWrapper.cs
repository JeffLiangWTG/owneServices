using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit;

[CodeAlive("Used in documents DataContext")]
[DefaultField(nameof(HumanReadableName)), WrapperTypeName("T2L")]
public class T2LDocWrapper : DocBaseWrapper
{
	T2LDocWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory, bool isForT2LF) : base(entryHeader, factory)
	{
		if (entryHeader != null)
		{
			t2LDocLines = new T2LDocLineWrapperCollection(EntryHeader.T2LApplicableEntryLines, Factory, isForT2LF);
		}
	}

	public static T2LDocWrapper New(CusEntryHeader entryHeader, BusinessObjectFactory factory) => new T2LDocWrapper(entryHeader, factory, false);

	public CusEntryHeader EntryHeader => (CusEntryHeader)base.WrappedObject;

	public T2LDocLineWrapperCollection T2LDocLines => t2LDocLines;
	public T2LDocLineWrapperCollection t2LDocLines;
}
