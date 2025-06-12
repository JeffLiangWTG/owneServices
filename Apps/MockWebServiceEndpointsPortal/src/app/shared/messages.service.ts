import {EndpointService} from './endpoint.service'
import {Message, Folder, AppSatus} from '../model/messaging-interface'
import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable()
export class MessagesManager {
	id: string = '';
	appStatus!: AppSatus;
	constructor(private endpointService: EndpointService) {

	}
	init(id: string, messageBoxStatus: AppSatus) {
		this.id = id;
		this.appStatus = messageBoxStatus;
	}

	load(callback: Function | undefined = undefined, reload: boolean = false): Promise<Message[]> {
		var processingMessage = !reload ? "loading messages" : undefined;
		var successMessage = !reload ? "messages loaded" : undefined;
		return this.HandleEndpointPromise(this.endpointService.getMessages(this.id), callback, processingMessage, successMessage);
	}

	loadInfo(callback: Function | undefined = undefined){
		var processingMessage = "loading info..."
		var successMessage = "retrieved info"
		return this.HandleEndpointPromise(this.endpointService.getMockEnpointInfo(this.id), callback, processingMessage, successMessage);
	}

	loadSubFolders(callback: Function | undefined = undefined, reload: boolean = false) {
		var processingMessage = !reload ? "loading folders" : undefined;
		var successMessage = !reload ? "folders loaded" : undefined;
		return this.HandleEndpointPromise(this.endpointService.getFolders(this.id), callback, processingMessage, successMessage);
	}

	upload(files: File[], callback: Function | undefined = undefined)
	{
		var formData = new FormData();

		files.forEach(file => {
			formData.append(file.name, file);
		});

		return this.HandleEndpointPromise(this.endpointService.uploadMessages(this.id, formData), callback, "uploading messages").then(result => result);
	}

	remove(message: Message, successCallback: Function) {
		var processingMessage = "deleting message " + message.name;
		var successMessage = "message deleted";
		return this.HandleEndpointPromise(this.endpointService.removeMessage(this.id, message.name), successCallback, processingMessage, successMessage).then(result => result);
	}

	removeDir(folder: Folder, successCallback: Function) {
		var processingMessage = "deleting directory " + folder.name;
		var successMessage = "folder deleted";
		return this.HandleEndpointPromise(this.endpointService.removeFolder(this.id, folder.name), successCallback, processingMessage, successMessage).then(result => result);
	}

	toggleActivate(message: Message, keepOriginal: string, successCallback: Function) {
		var processingMessage = "moving message " + message.name;
		var successMessage = "message moved";
		return this.HandleEndpointPromise(this.endpointService.toggleActivate(this.id, message.name, keepOriginal), successCallback, processingMessage, successMessage).then(result => result);
	}

	view(message: Message, callback: Function) {
		var processingMessage = "viewing message " + message.name;
		var successMessage = "message retrieved";

		return this.HandleEndpointPromise(this.endpointService.getMessageContent(this.id, message.name), callback, processingMessage, successMessage).then(result => result);
	}

	setInfo(args: any){
		if (args) {
			if (args.hasOwnProperty('busy')) {             
				this.appStatus.busy = args.busy;
			}
			if (args.hasOwnProperty('message')) {                    
				this.appStatus.message = args.message;
			}
		} else {
			this.appStatus.busy = false;
			this.appStatus.message = '';
		}
	}

	private HandleEndpointPromise(promise: Promise<any>, successCallback: Function | undefined = undefined, processingMessage: string | undefined = undefined, successMessage: string | undefined = undefined) {
		if (processingMessage) this.setInfo({busy: true, message: processingMessage});

		return promise.then(data => {
			if (successCallback) successCallback(data);
			if (successMessage) this.setInfo({message: successMessage});
			return data;
		}).catch((err: HttpErrorResponse) => this.handleError(err)).finally(() => this.setInfo({busy: false})); 
	}

	private handleError(error: HttpErrorResponse) {
		if (error.status === 0) {
			console.error('An error occurred:', error.error);
		} else {
			console.error(`Backend returned code ${error.status}, body was: `, error.error);
		}
		
		this.setInfo({message: 'Something bad happened; please try again later.'});
	}
}
