import { Injectable } from '@angular/core';
import { HttpClient, HttpRequest } from '@angular/common/http';
import { firstValueFrom  } from 'rxjs';
import { ConfigService } from './config.service';



@Injectable()
export class EndpointService{
	url: string = '';
	constructor(private http: HttpClient, configService: ConfigService){
		this.url = configService.config.mockEndpointsUrl;
	}

	query(path: string) {
		return firstValueFrom(this.http.get(this.url + path));
	}
	
	getEndpoints(type: string) : Promise<any> {
		return firstValueFrom(this.http.get(this.url + '/api/endpoints/' + type));
	}

	getMessages(id: string) {
		return this.query('/api/message/' + id);
	}

	getFolders(id: string) {
		return this.query('/api/message/' + id + '/folders');
	}

	getMessageContent(id: string, fileName: string) {
		return this.query(`/api/message/${id}/${fileName}`);
	}

	uploadMessages(id: string, formData: FormData) {
		return firstValueFrom(this.http.post(this.url + `/api/message/${id}`, formData));
	}

	removeMessage(id: string, fileName: string) {
		return firstValueFrom(this.http.delete(this.url + `/api/message/${id}/${fileName}`));
	}

	removeFolder(id: string, folderName: string) {
		return firstValueFrom(this.http.delete(this.url + `/api/message/${id}/folder/${folderName}`));
	}

	toggleActivate(id: string, fileName: string, active: string) {
		return firstValueFrom(this.http.put(this.url + `/api/message/${id}/${fileName}/${active}`, {}));
	}

	getMockEnpointInfo(id: string) {
		return this.query(`/${id}/info`);
	}
}