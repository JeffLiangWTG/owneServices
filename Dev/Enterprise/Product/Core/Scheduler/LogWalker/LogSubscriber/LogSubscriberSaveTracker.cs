using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.LogWalker
{
	public sealed class LogSubscriberSaveTracker
	{
		readonly StringBuilder errorMessage = new StringBuilder();

		public bool ReportConcurrencyErrorCausedByInvalidSave(ProcessableLogGroup currentLogGroup, Exception concurrencyException)
		{
			if (errorMessage.Length == 0)
			{
				return false;
			}

			errorMessage.Insert(0, System.Environment.NewLine);
			errorMessage.Insert(0, currentLogGroup.Subscriber.GetPrettyPrinter().PrettyPrintSubscriberErrorInfo(currentLogGroup.Logs.Single()));

			ExceptionReporter.Instance.ReportDeveloperException(errorMessage.ToString(), concurrencyException);
			return true;
		}

		public IDisposable TrackInvalidSaves()
		{
			return new FactorySaveAlerter(TrackInvalidSaves);
		}

		public void AddValidFactoryToSave(string factoryName)
		{
			validFactoryNames.Add(factoryName);
		}

		void TrackInvalidSaves(ITransactionParticipant[] participants)
		{
			foreach (var participant in participants)
			{
				var factory = participant as BusinessObjectFactory;

				if (factory != null && validFactoryNames.Contains(factory.NameForDebugging) && factory.ChildFactories.Count == 0)
				{
					//this is the log subscriber factory that should be used for saving
				}
				else
				{
					if (factory != null)
					{
						CollectFactoryInfo(factory, FactoryType.Main);
						factory.ChildFactories.Cast<BusinessObjectFactory>().ForEach(x => CollectFactoryInfo(x, FactoryType.Child));
					}
					else
					{
						CollectTransactionParticipantInfo(participant);
					}

					errorMessage.AppendLine();
					errorMessage.AppendLine("Stack Trace:");
					errorMessage.AppendLine((new StackTrace()).ToString());

					break;
				}
			}
		}

		void CollectFactoryInfo(BusinessObjectFactory factory, FactoryType factoryType)
		{
			var factoryName = factory.NameForDebugging;
			var allocationPath = factory.AllocationPath;
			var factoryTypeCaption = factoryType.ToString();

			errorMessage.AppendLine();
			errorMessage.AppendLine(factoryTypeCaption + " Factory Name: " + factoryName);
			errorMessage.AppendLine(factoryTypeCaption + " Factory Allocation Path:");
			errorMessage.AppendLine(allocationPath);
		}

		void CollectTransactionParticipantInfo(ITransactionParticipant participant)
		{
			errorMessage.AppendLine();
			errorMessage.AppendLine("Transaction Participant Type: " + participant.GetType().FullName);
		}

		enum FactoryType
		{
			Main,
			Child
		}

		readonly List<string> validFactoryNames = new List<string>();
	}
}
