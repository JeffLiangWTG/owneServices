using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("DE Customs Messaging")]
[assembly: AssemblyDescription("DE Customs Messaging")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.DE.Messaging.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: UsesConstants(typeof(EDIInterchange))]
[assembly: UsesConstants(typeof(EDIMessage))]
