using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class Document : IDocument
	{
		public Document(IDocumentDescriptor descriptor, IStmALogParent logParent, IReadOnlyCollection<IPage> pages = null)
		{
			Argument.NotNull(descriptor, nameof(descriptor));
			Argument.NotNull(logParent, nameof(logParent));

			this.descriptor = descriptor;
			this.logParent = logParent;
			this.Pages = pages ?? Array.Empty<IPage>();

			this.logs = new Lazy<IReadOnlyCollection<ILog>>(GetLogs);
		}

		readonly IDocumentDescriptor descriptor;
		readonly IStmALogParent logParent;
		readonly Lazy<IReadOnlyCollection<ILog>> logs;

		public string Name => descriptor.Name;
		public bool EnableTranslation => descriptor.EnableTranslation;
		public string MenuName => descriptor.MenuName;
		public string Type => descriptor.DocumentType;
		public IPrintInstructions PrintInstructions => descriptor.PrintInstructions;
		public IMessageInstructions MessageInstructions => descriptor.MessageInstructions;
		public IDisplayInstructions DisplayInstructions => descriptor.DisplayInstructions;

		public IReadOnlyCollection<IPage> Pages { get; }
		public IReadOnlyCollection<ILog> Logs => logs.Value;

		IReadOnlyCollection<ILog> GetLogs()
		{
			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => !log.IsCancelled)
				.OrderByDescending(l => l.SL_EventTime)
				.Select(l => new Log(l))
				.ToArray();
		}
	}
}
